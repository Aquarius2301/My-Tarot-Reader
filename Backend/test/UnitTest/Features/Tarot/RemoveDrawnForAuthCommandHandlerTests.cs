using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MyTarotReader.Application.Common.Exceptions;
using MyTarotReader.Application.Constants.Errors;
using MyTarotReader.Application.Features.Tarot;
using MyTarotReader.Domain.Entities;
using MyTarotReader.Domain.Enums;
using MyTarotReader.Infrastructure.Persistence;
using MyTarotReader.UnitTest.Common;
using Xunit;

namespace MyTarotReader.UnitTest.Features.Tarot;

public class RemoveDrawnForAuthCommandHandlerTests
{
    private const string ValidCard = "maj-00";

    private static RemoveDrawnForAuthCommandHandler CreateSut(AppDbContext db) => new(db);

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

    private static TarotReading SeedReading(AppDbContext db, Guid userId, string cardCode = ValidCard)
    {
        var reading = new TarotReading
        {
            UserId = userId,
            CardCode = cardCode,
            IsReversed = false,
        };
        db.TarotReadings.Add(reading);
        db.SaveChanges();
        return reading;
    }

    [Fact]
    public async Task RemoveDrawnForAuth_ExistingRecord_SoftDeletes()
    {
        // Arrange
        var db = TestDbContextFactory.Create();
        var userId = Guid.NewGuid();
        SeedUser(db, userId);
        var reading = SeedReading(db, userId);
        var sut = CreateSut(db);

        // Act
        await sut.Handle(new RemoveDrawnForAuthCommand(userId, reading.Id), CancellationToken.None);

        // Assert
        db.TarotReadings.IgnoreQueryFilters()
            .Single(r => r.Id == reading.Id)
            .DeletedAt.Should()
            .NotBeNull();
        db.TarotReadings.Should().BeEmpty();
    }

    [Fact]
    public async Task RemoveDrawnForAuth_ReadingIdNotExist_ThrowsNotFound()
    {
        // Arrange
        var db = TestDbContextFactory.Create();
        var sut = CreateSut(db);

        // Act
        var act = () =>
            sut.Handle(new RemoveDrawnForAuthCommand(Guid.NewGuid(), Guid.NewGuid()), CancellationToken.None);

        // Assert
        var ex = await act.Should().ThrowAsync<NotFoundException>();
        ex.Which.ErrorCode.Should().Be(TarotReadingErrorCode.NotFound);
    }

    [Fact]
    public async Task RemoveDrawnForAuth_RecordBelongsToOtherUser_ThrowsNotFound()
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
        var act = () => sut.Handle(new RemoveDrawnForAuthCommand(userA, readingB.Id), CancellationToken.None);

        // Assert
        var ex = await act.Should().ThrowAsync<NotFoundException>();
        ex.Which.ErrorCode.Should().Be(TarotReadingErrorCode.NotFound);
        db.TarotReadings.IgnoreQueryFilters()
            .Single(r => r.Id == readingB.Id)
            .DeletedAt.Should()
            .BeNull();
    }
}
