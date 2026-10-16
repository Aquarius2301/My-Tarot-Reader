using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MyTarotReader.Application.Common.Exceptions;
using MyTarotReader.Application.Constants.Errors;
using MyTarotReader.Application.Features.AiDeepTarot;
using MyTarotReader.Domain.Entities;
using MyTarotReader.Domain.Enums;
using MyTarotReader.Infrastructure.Persistence;
using MyTarotReader.UnitTest.Common;
using Xunit;

namespace MyTarotReader.UnitTest.Features.AiDeepTarot;

public class DeleteAiDeepTarotReadingCommandHandlerTests
{
    private static DeleteAiDeepTarotReadingCommandHandler CreateSut(AppDbContext db) => new(db);

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

    private static AIDeepTarotReading SeedReading(AppDbContext db, Guid userId, bool deleted = false)
    {
        var reading = new AIDeepTarotReading
        {
            UserId = userId,
            Topic = DeepTarotTopic.TwelveHouses,
            Title = "Vận mệnh qua 12 nhà",
            Answer = """{"title":"Vận mệnh qua 12 nhà","overview":"Một tổng quan.","sections":[],"overallAdvice":"x"}""",
            AnswerSummary = "Một tổng quan.",
            Cards = """[{"cardCode":"maj-00","isReversed":false}]""",
            CreatedAt = DateTimeOffset.UtcNow,
            DeletedAt = deleted ? DateTimeOffset.UtcNow : null,
        };
        db.AIDeepTarotReadings.Add(reading);
        db.SaveChanges();
        return reading;
    }

    [Fact]
    public async Task Handle_ExistingReading_SoftDeletes()
    {
        // Arrange
        var db = TestDbContextFactory.Create();
        var userId = Guid.NewGuid();
        SeedUser(db, userId);
        var reading = SeedReading(db, userId);
        var sut = CreateSut(db);

        // Act
        await sut.Handle(new DeleteAiDeepTarotReadingCommand(userId, reading.Id), CancellationToken.None);

        // Assert
        db.AIDeepTarotReadings.IgnoreQueryFilters()
            .Single(r => r.Id == reading.Id)
            .DeletedAt.Should()
            .NotBeNull();
        db.AIDeepTarotReadings.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ReadingNotFound_ThrowsNotFound()
    {
        // Arrange
        var db = TestDbContextFactory.Create();
        var sut = CreateSut(db);

        // Act
        var act = () => sut.Handle(new DeleteAiDeepTarotReadingCommand(Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None);

        // Assert
        var ex = await act.Should().ThrowAsync<NotFoundException>();
        ex.Which.ErrorCode.Should().Be(AiDeepTarotErrorCode.ReadingNotFound);
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
        var readingB = SeedReading(db, userB);
        var sut = CreateSut(db);

        // Act
        var act = () => sut.Handle(new DeleteAiDeepTarotReadingCommand(userA, readingB.Id), CancellationToken.None);

        // Assert
        var ex = await act.Should().ThrowAsync<NotFoundException>();
        ex.Which.ErrorCode.Should().Be(AiDeepTarotErrorCode.ReadingNotFound);
        db.AIDeepTarotReadings.IgnoreQueryFilters()
            .Single(r => r.Id == readingB.Id)
            .DeletedAt.Should()
            .BeNull();
    }
}
