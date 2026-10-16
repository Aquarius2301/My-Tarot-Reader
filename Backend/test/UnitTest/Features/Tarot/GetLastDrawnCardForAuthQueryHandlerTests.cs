using FluentAssertions;
using MyTarotReader.Application.Features.Tarot;
using MyTarotReader.Domain.Entities;
using MyTarotReader.Domain.Enums;
using MyTarotReader.Infrastructure.Persistence;
using MyTarotReader.UnitTest.Common;
using Xunit;

namespace MyTarotReader.UnitTest.Features.Tarot;

public class GetLastDrawnCardForAuthQueryHandlerTests
{
    private const string ValidCard = "maj-00";

    private static GetLastDrawnCardForAuthQueryHandler CreateSut(AppDbContext db) => new(db);

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

    [Fact]
    public async Task GetLastDrawnCardForAuth_HasDraw_ReturnsLatest()
    {
        // Arrange
        var db = TestDbContextFactory.Create();
        var userId = Guid.NewGuid();
        SeedUser(db, userId);
        db.TarotReadings.Add(
            new TarotReading
            {
                UserId = userId,
                CardCode = "maj-01",
                IsReversed = false,
                CreatedAt = DateTimeOffset.UtcNow.AddHours(-2),
            }
        );
        db.TarotReadings.Add(
            new TarotReading
            {
                UserId = userId,
                CardCode = ValidCard,
                IsReversed = true,
                CreatedAt = DateTimeOffset.UtcNow,
            }
        );
        db.SaveChanges();
        var sut = CreateSut(db);

        // Act
        var result = await sut.Handle(new GetLastDrawnCardForAuthQuery(userId), CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.CardCode.Should().Be(ValidCard);
        result.IsReversed.Should().BeTrue();
    }

    [Fact]
    public async Task GetLastDrawnCardForAuth_NoDraw_ReturnsNull()
    {
        // Arrange
        var db = TestDbContextFactory.Create();
        var sut = CreateSut(db);

        // Act
        var result = await sut.Handle(new GetLastDrawnCardForAuthQuery(Guid.NewGuid()), CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetLastDrawnCardForAuth_OnlyOwnUserDraws()
    {
        // Arrange
        var db = TestDbContextFactory.Create();
        var userA = Guid.NewGuid();
        var userB = Guid.NewGuid();
        SeedUser(db, userA);
        SeedUser(db, userB);
        db.TarotReadings.Add(
            new TarotReading
            {
                UserId = userA,
                CardCode = ValidCard,
                IsReversed = false,
                CreatedAt = DateTimeOffset.UtcNow.AddMinutes(-1),
            }
        );
        db.TarotReadings.Add(
            new TarotReading
            {
                UserId = userB,
                CardCode = "maj-21",
                IsReversed = true,
                CreatedAt = DateTimeOffset.UtcNow,
            }
        );
        db.SaveChanges();
        var sut = CreateSut(db);

        // Act
        var result = await sut.Handle(new GetLastDrawnCardForAuthQuery(userA), CancellationToken.None);

        // Assert
        result!.CardCode.Should().Be(ValidCard);
        result.IsReversed.Should().BeFalse();
    }
}
