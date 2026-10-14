using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MyTarotReader.Application.Features.Tarot;
using MyTarotReader.Domain.Entities;
using MyTarotReader.Domain.Enums;
using MyTarotReader.Infrastructure.Persistence;
using MyTarotReader.UnitTest.Common;
using Xunit;

namespace MyTarotReader.UnitTest.Features.Tarot;

public class GetAllReadingQueryHandlerTests
{
    private const string ValidCard = "maj-00";

    private static GetAllReadingQueryHandler CreateSut(AppDbContext db) => new(db);

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
    public async Task GetAllReading_UserHasHistory_ReturnsMappedHistory()
    {
        // Arrange
        var db = TestDbContextFactory.Create();
        var userId = Guid.NewGuid();
        SeedUser(db, userId);
        db.TarotReadings.Add(
            new TarotReading
            {
                UserId = userId,
                CardCode = ValidCard,
                IsReversed = false,
                CreatedAt = DateTimeOffset.UtcNow,
            }
        );
        db.TarotReadings.Add(
            new TarotReading
            {
                UserId = userId,
                CardCode = "min-wands-1",
                IsReversed = true,
                CreatedAt = DateTimeOffset.UtcNow.AddMinutes(-5),
            }
        );
        db.SaveChanges();
        var sut = CreateSut(db);

        // Act
        var result = await sut.Handle(new GetAllReadingQuery(1, 10, userId), CancellationToken.None);

        // Assert
        result.Items.Should().HaveCount(2);
        var first = result.Items.Single(x => x.CardCode == ValidCard);
        first.CardCode.Should().Be(ValidCard);
        first.IsReversed.Should().BeFalse();

        var second = result.Items.Single(x => x.CardCode == "min-wands-1");
        second.IsReversed.Should().BeTrue();
    }

    [Fact]
    public async Task GetAllReading_NoHistory_ReturnsEmptyList()
    {
        // Arrange
        var db = TestDbContextFactory.Create();
        var sut = CreateSut(db);

        // Act
        var result = await sut.Handle(new GetAllReadingQuery(1, 10, Guid.NewGuid()), CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllReading_OnlyReturnsUsersOwnHistory()
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
                CreatedAt = DateTimeOffset.UtcNow,
            }
        );
        db.TarotReadings.Add(
            new TarotReading
            {
                UserId = userA,
                CardCode = "min-wands-1",
                IsReversed = true,
                CreatedAt = DateTimeOffset.UtcNow.AddMinutes(-1),
            }
        );
        db.TarotReadings.Add(
            new TarotReading
            {
                UserId = userB,
                CardCode = "maj-10",
                IsReversed = false,
                CreatedAt = DateTimeOffset.UtcNow,
            }
        );
        db.SaveChanges();
        var sut = CreateSut(db);

        // Act
        var result = await sut.Handle(new GetAllReadingQuery(1, 10, userA), CancellationToken.None);

        // Assert
        result.Items.Should().HaveCount(2);
        result.Items.Should().AllSatisfy(x => x.CardCode.Should().BeOneOf(ValidCard, "min-wands-1"));
    }

    [Fact]
    public async Task GetAllReading_ReturnsOrderedByCreatedAtDescending()
    {
        // Arrange
        var db = TestDbContextFactory.Create();
        var userId = Guid.NewGuid();
        SeedUser(db, userId);
        var oldest = DateTimeOffset.UtcNow.AddDays(-2);
        var middle = DateTimeOffset.UtcNow.AddDays(-1);
        var newest = DateTimeOffset.UtcNow;
        db.TarotReadings.Add(
            new TarotReading
            {
                UserId = userId,
                CardCode = "oldest",
                IsReversed = false,
                CreatedAt = oldest,
            }
        );
        db.TarotReadings.Add(
            new TarotReading
            {
                UserId = userId,
                CardCode = "middle",
                IsReversed = true,
                CreatedAt = middle,
            }
        );
        db.TarotReadings.Add(
            new TarotReading
            {
                UserId = userId,
                CardCode = "newest",
                IsReversed = false,
                CreatedAt = newest,
            }
        );
        db.SaveChanges();
        var sut = CreateSut(db);

        // Act
        var result = await sut.Handle(new GetAllReadingQuery(1, 10, userId), CancellationToken.None);

        // Assert
        result.Items.Select(x => x.CreatedAt).Should().Equal(newest, middle, oldest);
    }

    [Fact]
    public async Task GetAllReading_ExcludesSoftDeletedRecords()
    {
        // Arrange
        var db = TestDbContextFactory.Create();
        var userId = Guid.NewGuid();
        SeedUser(db, userId);
        db.TarotReadings.Add(
            new TarotReading
            {
                UserId = userId,
                CardCode = "kept",
                IsReversed = false,
                CreatedAt = DateTimeOffset.UtcNow,
            }
        );
        var deleted = new TarotReading
        {
            UserId = userId,
            CardCode = "deleted",
            IsReversed = true,
            CreatedAt = DateTimeOffset.UtcNow,
        };
        db.TarotReadings.Add(deleted);
        db.SaveChanges();

        deleted.DeletedAt = DateTimeOffset.UtcNow;
        db.SaveChanges();
        var sut = CreateSut(db);

        // Act
        var result = await sut.Handle(new GetAllReadingQuery(1, 10, userId), CancellationToken.None);

        // Assert
        result.Items.Should().ContainSingle();
        result.Items.Single().CardCode.Should().Be("kept");
    }

    [Fact]
    public async Task GetAllReading_PageTwo_ReturnsSecondPage()
    {
        // Arrange
        var db = TestDbContextFactory.Create();
        var userId = Guid.NewGuid();
        SeedUser(db, userId);
        for (var i = 0; i < 5; i++)
        {
            db.TarotReadings.Add(
                new TarotReading
                {
                    UserId = userId,
                    CardCode = ValidCard,
                    IsReversed = false,
                    CreatedAt = DateTimeOffset.UtcNow.AddMinutes(-i),
                }
            );
        }
        db.SaveChanges();
        var sut = CreateSut(db);

        // Act
        var result = await sut.Handle(new GetAllReadingQuery(2, 2, userId), CancellationToken.None);

        // Assert
        result.Items.Should().HaveCount(2);
        result.TotalItems.Should().Be(5);
        result.Page.Should().Be(2);
        result.PageSize.Should().Be(2);
    }

    [Fact]
    public async Task GetAllReading_InvalidPagingInput_UsesDefaults()
    {
        // Arrange
        var db = TestDbContextFactory.Create();
        var userId = Guid.NewGuid();
        SeedUser(db, userId);
        db.TarotReadings.Add(
            new TarotReading
            {
                UserId = userId,
                CardCode = ValidCard,
                IsReversed = false,
                CreatedAt = DateTimeOffset.UtcNow,
            }
        );
        db.SaveChanges();
        var sut = CreateSut(db);

        // Act
        var result = await sut.Handle(new GetAllReadingQuery(0, -5, userId), CancellationToken.None);

        // Assert
        result.Items.Should().ContainSingle();
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(10);
    }

    [Fact]
    public async Task GetAllReading_TooLargePageSize_ClampedToMax()
    {
        // Arrange
        var db = TestDbContextFactory.Create();
        var userId = Guid.NewGuid();
        SeedUser(db, userId);
        db.TarotReadings.Add(
            new TarotReading
            {
                UserId = userId,
                CardCode = ValidCard,
                IsReversed = false,
                CreatedAt = DateTimeOffset.UtcNow,
            }
        );
        db.SaveChanges();
        var sut = CreateSut(db);

        // Act
        var result = await sut.Handle(new GetAllReadingQuery(1, 500, userId), CancellationToken.None);

        // Assert
        result.Items.Should().ContainSingle();
        result.PageSize.Should().Be(50);
    }
}
