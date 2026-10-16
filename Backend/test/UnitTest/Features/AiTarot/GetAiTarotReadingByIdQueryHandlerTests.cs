using System.Text.Json;
using FluentAssertions;
using MyTarotReader.Application.Common.Exceptions;
using MyTarotReader.Application.Constants.Errors;
using MyTarotReader.Application.Features.AiTarot;
using MyTarotReader.Domain.Entities;
using MyTarotReader.Domain.Enums;
using MyTarotReader.Infrastructure.Persistence;
using MyTarotReader.UnitTest.Common;
using Xunit;

namespace MyTarotReader.UnitTest.Features.AiTarot;

public class GetAiTarotReadingByIdQueryHandlerTests
{
    private const string ValidCard = "maj-00";

    private static GetAiTarotReadingByIdQueryHandler CreateSut(AppDbContext db) => new(db);

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

    private static string BuildGeminiJson(string overview, string cardCode = ValidCard) =>
        JsonSerializer.Serialize(
            new
            {
                title = "Tình yêu sắp tới",
                overview,
                cards = new[]
                {
                    new
                    {
                        cardCode,
                        position = "Quá khứ",
                        interpretation = "Bạn đang bắt đầu một giai đoạn mới.",
                    },
                },
                overallAdvice = "Hãy lắng nghe trực giác của bạn.",
            }
        );

    private static AITarotReading SeedReading(
        AppDbContext db,
        Guid userId,
        string cardsJson,
        string? answer = null,
        bool deleted = false
    )
    {
        var reading = new AITarotReading
        {
            UserId = userId,
            Title = "Tình yêu sắp tới",
            CardCount = 3,
            QuestionType = "love",
            Answer = answer ?? BuildGeminiJson("Một câu trả lời dài đầy đủ."),
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
    public async Task Handle_Exists_ReturnsFullReading()
    {
        // Arrange
        var db = TestDbContextFactory.Create();
        var userId = Guid.NewGuid();
        SeedUser(db, userId);
        var reading = SeedReading(db, userId, """[{"cardCode":"maj-00","isReversed":false}]""");
        var sut = CreateSut(db);

        // Act
        var result = await sut.Handle(new GetAiTarotReadingByIdQuery(userId, reading.Id), CancellationToken.None);

        // Assert
        result.Id.Should().Be(reading.Id);
        result.Title.Should().Be(reading.Title);
        result.CardCount.Should().Be(3);
        result.Type.Should().Be("love");
        result.Answer.Should().NotBeNullOrEmpty();
        JsonDocument.Parse(result.Answer)
            .RootElement.GetProperty("overview")
            .GetString()
            .Should()
            .Contain("trả lời dài đầy đủ");
        result.Cards.Should().ContainSingle();
        result.Cards[0].CardCode.Should().Be(ValidCard);
        result.Cards[0].IsReversed.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_StoredAnswerWithNameCode_ReturnsCanonicalCode()
    {
        // Arrange
        var db = TestDbContextFactory.Create();
        var userId = Guid.NewGuid();
        SeedUser(db, userId);
        var reading = SeedReading(
            db,
            userId,
            """[{"cardCode":"maj-00","isReversed":false}]""",
            answer: BuildGeminiJson("Một câu trả lời dài đầy đủ.", cardCode: "the_fool")
        );
        var sut = CreateSut(db);

        // Act
        var result = await sut.Handle(new GetAiTarotReadingByIdQuery(userId, reading.Id), CancellationToken.None);

        // Assert
        JsonDocument.Parse(result.Answer)
            .RootElement.GetProperty("cards")[0]
            .GetProperty("cardCode")
            .GetString()
            .Should()
            .Be("maj-00");
    }

    [Fact]
    public async Task Handle_NotFound_ThrowsNotFound()
    {
        // Arrange
        var db = TestDbContextFactory.Create();
        var sut = CreateSut(db);

        // Act
        var act = () => sut.Handle(new GetAiTarotReadingByIdQuery(Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None);

        // Assert
        var ex = await act.Should().ThrowAsync<NotFoundException>();
        ex.Which.ErrorCode.Should().Be(AiTarotErrorCode.ReadingNotFound);
    }

    [Fact]
    public async Task Handle_BelongsToOtherUser_ThrowsNotFound()
    {
        // Arrange
        var db = TestDbContextFactory.Create();
        var userA = Guid.NewGuid();
        var userB = Guid.NewGuid();
        SeedUser(db, userA);
        SeedUser(db, userB);
        var readingB = SeedReading(db, userB, """[{"cardCode":"maj-00","isReversed":false}]""");
        var sut = CreateSut(db);

        // Act
        var act = () => sut.Handle(new GetAiTarotReadingByIdQuery(userA, readingB.Id), CancellationToken.None);

        // Assert
        var ex = await act.Should().ThrowAsync<NotFoundException>();
        ex.Which.ErrorCode.Should().Be(AiTarotErrorCode.ReadingNotFound);
    }

    [Fact]
    public async Task Handle_SoftDeleted_ThrowsNotFound()
    {
        // Arrange
        var db = TestDbContextFactory.Create();
        var userId = Guid.NewGuid();
        SeedUser(db, userId);
        var reading = SeedReading(db, userId, """[{"cardCode":"maj-00","isReversed":false}]""", deleted: true);
        var sut = CreateSut(db);

        // Act
        var act = () => sut.Handle(new GetAiTarotReadingByIdQuery(userId, reading.Id), CancellationToken.None);

        // Assert
        var ex = await act.Should().ThrowAsync<NotFoundException>();
        ex.Which.ErrorCode.Should().Be(AiTarotErrorCode.ReadingNotFound);
    }
}
