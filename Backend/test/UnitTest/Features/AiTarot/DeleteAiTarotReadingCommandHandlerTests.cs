using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MyTarotReader.Application.Common.Exceptions;
using MyTarotReader.Application.Constants.Errors;
using MyTarotReader.Application.Features.AiTarot;
using MyTarotReader.Domain.Entities;
using MyTarotReader.Domain.Enums;
using MyTarotReader.Infrastructure.Persistence;
using MyTarotReader.UnitTest.Common;
using Xunit;

namespace MyTarotReader.UnitTest.Features.AiTarot;

public class DeleteAiTarotReadingCommandHandlerTests
{
    private static DeleteAiTarotReadingCommandHandler CreateSut(AppDbContext db) => new(db);

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

    private static AITarotReading SeedReading(AppDbContext db, Guid userId, bool deleted = false)
    {
        var reading = new AITarotReading
        {
            UserId = userId,
            Title = "Tình yêu sắp tới",
            CardCount = CardCount.Three,
            QuestionType = QuestionType.Love,
            Answer = "{}",
            AnswerSummary = "Tóm tắt.",
            Cards = """[{"cardCode":"maj-00","isReversed":false}]""",
            CreatedAt = DateTimeOffset.UtcNow,
            DeletedAt = deleted ? DateTimeOffset.UtcNow : null,
        };
        db.AITarotReadings.Add(reading);
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
        await sut.Handle(new DeleteAiTarotReadingCommand(userId, reading.Id), CancellationToken.None);

        // Assert
        db.AITarotReadings.IgnoreQueryFilters()
            .Single(r => r.Id == reading.Id)
            .DeletedAt.Should()
            .NotBeNull();
        db.AITarotReadings.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ReadingNotFound_ThrowsNotFound()
    {
        // Arrange
        var db = TestDbContextFactory.Create();
        var sut = CreateSut(db);

        // Act
        var act = () => sut.Handle(new DeleteAiTarotReadingCommand(Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None);

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
        var readingB = SeedReading(db, userB);
        var sut = CreateSut(db);

        // Act
        var act = () => sut.Handle(new DeleteAiTarotReadingCommand(userA, readingB.Id), CancellationToken.None);

        // Assert
        var ex = await act.Should().ThrowAsync<NotFoundException>();
        ex.Which.ErrorCode.Should().Be(AiTarotErrorCode.ReadingNotFound);
        db.AITarotReadings.IgnoreQueryFilters()
            .Single(r => r.Id == readingB.Id)
            .DeletedAt.Should()
            .BeNull();
    }
}
