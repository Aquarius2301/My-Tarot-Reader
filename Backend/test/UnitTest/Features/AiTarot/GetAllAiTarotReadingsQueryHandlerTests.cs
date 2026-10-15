using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MyTarotReader.Application.Features.AiTarot;
using MyTarotReader.Domain.Entities;
using MyTarotReader.Domain.Enums;
using MyTarotReader.Infrastructure.Persistence;
using MyTarotReader.UnitTest.Common;
using Xunit;

namespace MyTarotReader.UnitTest.Features.AiTarot;

public class GetAllAiTarotReadingsQueryHandlerTests
{
    private const string ValidCard = "maj-00";

    private static GetAllAiTarotReadingQueryHandler CreateSut(AppDbContext db) => new(db);

    private static void SeedUser(AppDbContext db, Guid userId)
    {
        db.Users.Add(
            new User
            {
                Id = userId,
                FullName = $"User-{userId:N}",
                Email = $"user-{userId:N}@example.com",
                Picture = "http://pic",
                ProviderKey = $"provider-{userId:N}",
                Role = UserRole.Registered,
            }
        );
        db.SaveChanges();
    }

    private static AITarotReading SeedReading(
        AppDbContext db,
        Guid userId,
        string cardsJson,
        bool deleted = false
    )
    {
        var reading = new AITarotReading
        {
            UserId = userId,
            Title = "Tình yêu sắp tới",
            CardCount = CardCount.Three,
            QuestionType = QuestionType.Love,
            Answer = """{"title":"Tình yêu sắp tới","overview":"Một câu trả lời dài đầy đủ.","cards":[],"overallAdvice":"x"}""",
            AnswerSummary = "Một câu trả lời dài đầy đủ.",
            Cards = cardsJson,
            CreatedAt = DateTimeOffset.UtcNow,
            DeletedAt = deleted ? DateTimeOffset.UtcNow : null,
        };
        db.AITarotReadings.Add(reading);
        db.SaveChanges();
        return reading;
    }

    [Fact]
    public async Task Handle_HasReadings_ReturnsItemsWithoutFullAnswer()
    {
        // Arrange
        var db = TestDbContextFactory.Create();
        var userId = Guid.NewGuid();
        SeedUser(db, userId);
        var newest = SeedReading(db, userId, """[{"cardCode":"maj-00","isReversed":false}]""");
        var oldest = SeedReading(db, userId, """[{"cardCode":"maj-06","isReversed":true}]""");
        oldest.CreatedAt = DateTimeOffset.UtcNow.AddHours(-1);
        db.SaveChanges();
        var sut = CreateSut(db);

        // Act
        var result = await sut.Handle(new GetAllAiTarotReadingQuery(1, 10, userId), CancellationToken.None);

        // Assert
        result.Items.Should().HaveCount(2);
        result.Items[0].Id.Should().Be(newest.Id);
        result.Items[0].AnswerSummary.Should().Be("Một câu trả lời dài đầy đủ.");
        result.Items[0].Cards.Should().ContainSingle(x => x.CardCode == ValidCard);
        typeof(GetAllAiTarotReadingItem)
            .GetProperty("Answer")
            .Should()
            .BeNull("the list DTO must not carry the full answer");
    }

    [Fact]
    public async Task Handle_NoReadings_ReturnsEmptyList()
    {
        // Arrange
        var db = TestDbContextFactory.Create();
        var sut = CreateSut(db);

        // Act
        var result = await sut.Handle(new GetAllAiTarotReadingQuery(1, 10, Guid.NewGuid()), CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_OnlyReturnsOwnReadings()
    {
        // Arrange
        var db = TestDbContextFactory.Create();
        var userA = Guid.NewGuid();
        var userB = Guid.NewGuid();
        SeedUser(db, userA);
        SeedUser(db, userB);
        SeedReading(db, userA, """[{"cardCode":"maj-00","isReversed":false}]""");
        SeedReading(db, userB, """[{"cardCode":"maj-21","isReversed":true}]""");
        var sut = CreateSut(db);

        // Act
        var result = await sut.Handle(new GetAllAiTarotReadingQuery(1, 10, userA), CancellationToken.None);

        // Assert
        result.Items.Should().ContainSingle();
        result.Items.Single().Cards.Single().CardCode.Should().Be(ValidCard);
    }

    [Fact]
    public async Task Handle_ExcludesSoftDeletedRecords()
    {
        // Arrange
        var db = TestDbContextFactory.Create();
        var userId = Guid.NewGuid();
        SeedUser(db, userId);
        SeedReading(db, userId, """[{"cardCode":"maj-00","isReversed":false}]""");
        SeedReading(db, userId, """[{"cardCode":"maj-06","isReversed":true}]""", deleted: true);
        var sut = CreateSut(db);

        // Act
        var result = await sut.Handle(new GetAllAiTarotReadingQuery(1, 10, userId), CancellationToken.None);

        // Assert
        result.Items.Should().ContainSingle();
        result.Items.Single().Cards.Single().CardCode.Should().Be(ValidCard);
    }

    [Fact]
    public async Task Handle_ReturnsOrderedByCreatedAtDescending()
    {
        // Arrange
        var db = TestDbContextFactory.Create();
        var userId = Guid.NewGuid();
        SeedUser(db, userId);
        var oldest = SeedReading(db, userId, """[{"cardCode":"maj-00","isReversed":false}]""");
        var newest = SeedReading(db, userId, """[{"cardCode":"maj-06","isReversed":true}]""");
        oldest.CreatedAt = DateTimeOffset.UtcNow.AddHours(-2);
        newest.CreatedAt = DateTimeOffset.UtcNow;
        db.SaveChanges();
        var sut = CreateSut(db);

        // Act
        var result = await sut.Handle(new GetAllAiTarotReadingQuery(1, 10, userId), CancellationToken.None);

        // Assert
        result.Items.Select(x => x.Id).Should().Equal(newest.Id, oldest.Id);
    }

    [Fact]
    public async Task Handle_PageTwo_ReturnsSecondPage()
    {
        // Arrange
        var db = TestDbContextFactory.Create();
        var userId = Guid.NewGuid();
        SeedUser(db, userId);
        for (var i = 0; i < 5; i++)
        {
            SeedReading(db, userId, """[{"cardCode":"maj-00","isReversed":false}]""");
        }

        var sut = CreateSut(db);

        // Act
        var result = await sut.Handle(new GetAllAiTarotReadingQuery(2, 2, userId), CancellationToken.None);

        // Assert
        result.Items.Should().HaveCount(2);
        result.Total.Should().Be(5);
        result.Page.Should().Be(2);
        result.PageSize.Should().Be(2);
    }

    [Fact]
    public async Task Handle_InvalidPagingInput_UsesDefaults()
    {
        // Arrange
        var db = TestDbContextFactory.Create();
        var userId = Guid.NewGuid();
        SeedUser(db, userId);
        SeedReading(db, userId, """[{"cardCode":"maj-00","isReversed":false}]""");
        var sut = CreateSut(db);

        // Act
        var result = await sut.Handle(new GetAllAiTarotReadingQuery(0, -5, userId), CancellationToken.None);

        // Assert
        result.Items.Should().ContainSingle();
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(10);
    }
}
