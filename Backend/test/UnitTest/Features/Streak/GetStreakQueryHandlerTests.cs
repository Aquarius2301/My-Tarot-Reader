using FluentAssertions;
using Microsoft.Extensions.Options;
using MyTarotReader.Application.Common.Helpers;
using MyTarotReader.Application.Features.Streak;
using MyTarotReader.Application.Settings;
using MyTarotReader.Infrastructure.Persistence;
using MyTarotReader.UnitTest.Common;
using Xunit;
using StreakEntity = MyTarotReader.Domain.Entities.Streak;

namespace MyTarotReader.UnitTest.Features.Streak;

public class GetStreakQueryHandlerTests
{
    private static GetStreakQueryHandler CreateSut(AppDbContext db) =>
        new(db, Options.Create(new StreakSetting()));

    private static StreakEntity SeedStreak(
        AppDbContext db,
        Guid userId,
        DateTime lastCheckIn,
        int current = 1,
        int longest = 1,
        int cycleDay = 1,
        bool saver = false
    )
    {
        var streak = new StreakEntity
        {
            UserId = userId,
            CurrentStreak = current,
            LongestStreak = longest,
            CycleDay = cycleDay,
            LastCheckIn = lastCheckIn,
            IsSaverUsed = saver,
        };
        db.Streaks.Add(streak);
        db.SaveChanges();
        return streak;
    }

    [Fact]
    public async Task GetStreak_NoStreak_ReturnsDefaultValues()
    {
        // Arrange
        var db = TestDbContextFactory.Create();
        var sut = CreateSut(db);

        // Act
        var result = await sut.Handle(new GetStreakQuery(Guid.NewGuid()), CancellationToken.None);

        // Assert
        result.CycleDay.Should().Be(0);
        result.CurrentStreak.Should().Be(0);
        result.LongestStreak.Should().Be(0);
        result.IsSaverUsed.Should().BeFalse();
        result.IsCheckedInToday.Should().BeFalse();
    }

    [Fact]
    public async Task GetStreak_CheckedInToday_ReturnsValues()
    {
        // Arrange
        var db = TestDbContextFactory.Create();
        var userId = Guid.NewGuid();
        SeedStreak(
            db,
            userId,
            StreakHelper.GetVietnamNow(),
            current: 5,
            longest: 8,
            cycleDay: 5
        );
        var sut = CreateSut(db);

        // Act
        var result = await sut.Handle(new GetStreakQuery(userId), CancellationToken.None);

        // Assert
        result.CycleDay.Should().Be(5);
        result.CurrentStreak.Should().Be(5);
        result.LongestStreak.Should().Be(8);
        result.IsCheckedInToday.Should().BeTrue();
    }

    [Fact]
    public async Task GetStreak_CycleDaySeven_ShowsZero()
    {
        // Arrange
        var db = TestDbContextFactory.Create();
        var userId = Guid.NewGuid();
        SeedStreak(db, userId, StreakHelper.GetVietnamNow(), cycleDay: 7);
        var sut = CreateSut(db);

        // Act
        var result = await sut.Handle(new GetStreakQuery(userId), CancellationToken.None);

        // Assert
        result.CycleDay.Should().Be(0);
    }

    [Fact]
    public async Task GetStreak_BrokenStreak_ResetsToZeroAndPersists()
    {
        // Arrange
        var db = TestDbContextFactory.Create();
        var userId = Guid.NewGuid();
        var streak = SeedStreak(
            db,
            userId,
            StreakHelper.GetVietnamNow().AddDays(-3),
            current: 5,
            longest: 8,
            cycleDay: 3
        );
        var sut = CreateSut(db);

        // Act
        var result = await sut.Handle(new GetStreakQuery(userId), CancellationToken.None);

        // Assert
        result.CurrentStreak.Should().Be(0);
        result.CycleDay.Should().Be(0);
        result.LongestStreak.Should().Be(8);
        streak.CurrentStreak.Should().Be(0);
        streak.CycleDay.Should().Be(0);
    }

    [Fact]
    public async Task GetStreak_BrokenStreak_WithinSameMonth_MarksSaverUsed()
    {
        // Arrange
        var db = TestDbContextFactory.Create();
        var userId = Guid.NewGuid();
        var streak = SeedStreak(
            db,
            userId,
            StreakHelper.GetVietnamNow().AddDays(-3),
            saver: false
        );
        var sut = CreateSut(db);

        // Act
        var result = await sut.Handle(new GetStreakQuery(userId), CancellationToken.None);

        // Assert
        result.IsSaverUsed.Should().BeTrue();
        streak.IsSaverUsed.Should().BeTrue();
    }

    [Fact]
    public async Task GetStreak_NewMonth_ResetsSaverAndPersists()
    {
        // Arrange
        var db = TestDbContextFactory.Create();
        var userId = Guid.NewGuid();
        var now = StreakHelper.GetVietnamNow();
        var lastMonth = new DateTime(now.Year, now.Month, 1).AddMonths(-1);
        var streak = SeedStreak(db, userId, lastMonth, current: 5, saver: true);
        var sut = CreateSut(db);

        // Act
        var result = await sut.Handle(new GetStreakQuery(userId), CancellationToken.None);

        // Assert
        result.IsSaverUsed.Should().BeFalse();
        streak.IsSaverUsed.Should().BeFalse();
    }
}
