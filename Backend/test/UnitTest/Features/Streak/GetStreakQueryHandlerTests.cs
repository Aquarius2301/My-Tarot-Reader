using FluentAssertions;
using Moq;
using MyTarotReader.Application.Common.Time;
using MyTarotReader.Application.Features.Streak;
using StreakEntity = MyTarotReader.Domain.Entities.Streak;
using MyTarotReader.Infrastructure.Persistence;
using MyTarotReader.UnitTest.Common;
using Xunit;

namespace MyTarotReader.UnitTest.Features.Streak;

public class GetStreakQueryHandlerTests
{
    private static GetStreakQueryHandler CreateSut(
        AppDbContext db,
        DateTimeOffset? fixedVietnamNow = null)
    {
        var timeProviderMock = new Mock<ITimeProvider>();
        timeProviderMock
            .Setup(t => t.GetVietnamNow())
            .Returns(fixedVietnamNow ?? DateTimeOffset.UtcNow);

        return new GetStreakQueryHandler(db, timeProviderMock.Object);
    }

    private static StreakEntity SeedStreak(
        AppDbContext db,
        Guid userId,
        int currentStreak,
        int longestStreak,
        int cycleDay,
        DateTime lastCheckIn,
        bool isSaverUsed)
    {
        var streak = new StreakEntity
        {
            UserId = userId,
            CurrentStreak = currentStreak,
            LongestStreak = longestStreak,
            CycleDay = cycleDay,
            LastCheckIn = lastCheckIn,
            IsSaverUsed = isSaverUsed
        };
        db.Streaks.Add(streak);
        db.SaveChanges();
        return streak;
    }

    [Fact]
    public async Task Handle_NewUser_ReturnsDefaultValues()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var db = TestDbContextFactory.Create();
        var vietnamNow = new DateTimeOffset(2026, 10, 22, 10, 0, 0, TimeSpan.Zero);
        var sut = CreateSut(db, vietnamNow);

        // Act
        var result = await sut.Handle(new GetStreakQuery(userId), CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.CurrentStreak.Should().Be(0);
        result.CycleDay.Should().Be(0);
        result.LongestStreak.Should().Be(0);
        result.IsSaverUsed.Should().BeFalse();
        result.IsCheckedInToday.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_Gap0_ReturnsIsCheckedInTodayTrue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var db = TestDbContextFactory.Create();
        var vietnamNow = new DateTimeOffset(2026, 10, 22, 10, 0, 0, TimeSpan.Zero);
        SeedStreak(db, userId, 5, 10, 4, vietnamNow.DateTime, false);
        var sut = CreateSut(db, vietnamNow);

        // Act
        var result = await sut.Handle(new GetStreakQuery(userId), CancellationToken.None);

        // Assert
        result.IsCheckedInToday.Should().BeTrue();
        result.CurrentStreak.Should().Be(5);
        result.CycleDay.Should().Be(4);
    }

    [Fact]
    public async Task Handle_Gap1_ReturnsIsCheckedInTodayFalse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var db = TestDbContextFactory.Create();
        var vietnamNow = new DateTimeOffset(2026, 10, 22, 10, 0, 0, TimeSpan.Zero);
        var yesterday = vietnamNow.AddDays(-1);
        SeedStreak(db, userId, 5, 10, 4, yesterday.DateTime, false);
        var sut = CreateSut(db, vietnamNow);

        // Act
        var result = await sut.Handle(new GetStreakQuery(userId), CancellationToken.None);

        // Assert
        result.IsCheckedInToday.Should().BeFalse();
        result.CurrentStreak.Should().Be(5);
        result.CycleDay.Should().Be(4);
    }

    [Fact]
    public async Task Handle_Gap2_SameMonth_SaverUnused_StreakGreaterThan1_ShowsSaverAvailable()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var db = TestDbContextFactory.Create();
        var vietnamNow = new DateTimeOffset(2026, 10, 22, 10, 0, 0, TimeSpan.Zero);
        var twoDaysAgo = vietnamNow.AddDays(-2);
        SeedStreak(db, userId, 3, 10, 2, twoDaysAgo.DateTime, false);
        var sut = CreateSut(db, vietnamNow);

        // Act
        var result = await sut.Handle(new GetStreakQuery(userId), CancellationToken.None);

        // Assert
        result.IsCheckedInToday.Should().BeFalse();
        result.CurrentStreak.Should().Be(3);
        result.CycleDay.Should().Be(2);
        result.IsSaverUsed.Should().BeTrue(); // Saver available to use
    }

    [Fact]
    public async Task Handle_Gap2_SameMonth_SaverUnused_Streak1OrLess_ResetsStreak()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var db = TestDbContextFactory.Create();
        var vietnamNow = new DateTimeOffset(2026, 10, 22, 10, 0, 0, TimeSpan.Zero);
        var twoDaysAgo = vietnamNow.AddDays(-2);
        SeedStreak(db, userId, 1, 10, 0, twoDaysAgo.DateTime, false);
        var sut = CreateSut(db, vietnamNow);

        // Act
        var result = await sut.Handle(new GetStreakQuery(userId), CancellationToken.None);

// Assert
        result.IsCheckedInToday.Should().BeFalse();
        result.CurrentStreak.Should().Be(0);
        result.CycleDay.Should().Be(0);
        result.IsSaverUsed.Should().BeFalse(); // No streak to save (<=1), saver not triggered
    }

    [Fact]
    public async Task Handle_Gap2_SameMonth_SaverUsed_ResetsStreak()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var db = TestDbContextFactory.Create();
        var vietnamNow = new DateTimeOffset(2026, 10, 22, 10, 0, 0, TimeSpan.Zero);
        var twoDaysAgo = vietnamNow.AddDays(-2);
        SeedStreak(db, userId, 5, 10, 4, twoDaysAgo.DateTime, true);
        var sut = CreateSut(db, vietnamNow);

        // Act
        var result = await sut.Handle(new GetStreakQuery(userId), CancellationToken.None);

// Assert
        result.IsCheckedInToday.Should().BeFalse();
        result.CurrentStreak.Should().Be(0);
        result.CycleDay.Should().Be(0);
        result.IsSaverUsed.Should().BeTrue(); // Saver preserved in same month
    }

    [Fact]
    public async Task Handle_Gap2_CrossMonth_ResetsStreakAndResetsSaver()
    {
        // Arrange - Dec 31 -> Jan 2 (cross month boundary)
        var userId = Guid.NewGuid();
        var db = TestDbContextFactory.Create();
        var vietnamNow = new DateTimeOffset(2027, 1, 2, 10, 0, 0, TimeSpan.Zero);
        var dec31 = new DateTimeOffset(2026, 12, 31, 10, 0, 0, TimeSpan.Zero);
        SeedStreak(db, userId, 5, 10, 4, dec31.DateTime, false);
        var sut = CreateSut(db, vietnamNow);

        // Act
        var result = await sut.Handle(new GetStreakQuery(userId), CancellationToken.None);

        // Assert
        result.IsCheckedInToday.Should().BeFalse();
        result.CurrentStreak.Should().Be(0);
        result.CycleDay.Should().Be(0);
        result.IsSaverUsed.Should().BeFalse(); // Saver resets on new month
    }

    [Fact]
    public async Task Handle_Gap3OrMore_ResetsStreakAndResetsSaverOnNewMonth()
    {
        // Arrange - Cross month: Dec 28 -> Jan 2 (gap=5, new month)
        var userId = Guid.NewGuid();
        var db = TestDbContextFactory.Create();
        var vietnamNow = new DateTimeOffset(2027, 1, 2, 10, 0, 0, TimeSpan.Zero);
        var dec28 = new DateTimeOffset(2026, 12, 28, 10, 0, 0, TimeSpan.Zero);
        SeedStreak(db, userId, 5, 10, 4, dec28.DateTime, true); // Saver was used
        var sut = CreateSut(db, vietnamNow);

        // Act
        var result = await sut.Handle(new GetStreakQuery(userId), CancellationToken.None);

        // Assert
        result.IsCheckedInToday.Should().BeFalse();
        result.CurrentStreak.Should().Be(0);
        result.CycleDay.Should().Be(0);
        result.IsSaverUsed.Should().BeFalse(); // Saver resets on new month
    }

    [Fact]
    public async Task Handle_Gap3OrMore_SameMonth_PreservesSaverUsed()
    {
        // Arrange - Same month gap >= 3
        var userId = Guid.NewGuid();
        var db = TestDbContextFactory.Create();
        var vietnamNow = new DateTimeOffset(2026, 10, 22, 10, 0, 0, TimeSpan.Zero);
        var fourDaysAgo = vietnamNow.AddDays(-4);
        SeedStreak(db, userId, 5, 10, 4, fourDaysAgo.DateTime, true); // Saver was used
        var sut = CreateSut(db, vietnamNow);

        // Act
        var result = await sut.Handle(new GetStreakQuery(userId), CancellationToken.None);

// Assert
        result.IsCheckedInToday.Should().BeFalse();
        result.CurrentStreak.Should().Be(0);
        result.CycleDay.Should().Be(0);
        result.IsSaverUsed.Should().BeTrue(); // Saver preserved in same month
    }

    [Fact]
    public async Task Handle_PreservesLongestStreak()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var db = TestDbContextFactory.Create();
        var vietnamNow = new DateTimeOffset(2026, 10, 22, 10, 0, 0, TimeSpan.Zero);
        var fourDaysAgo = vietnamNow.AddDays(-4);
        SeedStreak(db, userId, 5, 15, 4, fourDaysAgo.DateTime, false);
        var sut = CreateSut(db, vietnamNow);

        // Act
        var result = await sut.Handle(new GetStreakQuery(userId), CancellationToken.None);

        // Assert
        result.LongestStreak.Should().Be(15);
    }

    [Fact]
    public async Task Handle_Gap1_CrossMonth_ShowsSaverAsFalse()
    {
        // Arrange - Dec 31 -> Jan 1 (cross month boundary, consecutive day)
        var userId = Guid.NewGuid();
        var db = TestDbContextFactory.Create();
        var vietnamNow = new DateTimeOffset(2027, 1, 1, 10, 0, 0, TimeSpan.Zero);
        var dec31 = new DateTimeOffset(2026, 12, 31, 10, 0, 0, TimeSpan.Zero);
        SeedStreak(db, userId, 5, 10, 4, dec31.DateTime, true); // Saver was used last month
        var sut = CreateSut(db, vietnamNow);

        // Act
        var result = await sut.Handle(new GetStreakQuery(userId), CancellationToken.None);

        // Assert
        result.IsCheckedInToday.Should().BeFalse();
        result.CurrentStreak.Should().Be(5);
        result.CycleDay.Should().Be(4);
        result.IsSaverUsed.Should().BeFalse(); // Saver reset for new month display
    }

    [Fact]
    public async Task Handle_Gap2_CrossMonth_SaverAlreadyUsed_ShowsStreakZeroAndSaverFalse()
    {
        // Arrange - Dec 30 -> Jan 1 (cross month boundary, gap=2, saver already used)
        var userId = Guid.NewGuid();
        var db = TestDbContextFactory.Create();
        var vietnamNow = new DateTimeOffset(2027, 1, 1, 10, 0, 0, TimeSpan.Zero);
        var dec30 = new DateTimeOffset(2026, 12, 30, 10, 0, 0, TimeSpan.Zero);
        SeedStreak(db, userId, 5, 10, 4, dec30.DateTime, true); // Saver already used
        var sut = CreateSut(db, vietnamNow);

        // Act
        var result = await sut.Handle(new GetStreakQuery(userId), CancellationToken.None);

        // Assert
        result.IsCheckedInToday.Should().BeFalse();
        result.CurrentStreak.Should().Be(0);
        result.CycleDay.Should().Be(0);
        result.IsSaverUsed.Should().BeFalse(); // Saver resets on new month
    }

    [Fact]
    public async Task Handle_Gap3OrMore_NewMonth_SaverPreviouslyUsed_ShowsSaverAsFalse()
    {
        // Arrange - Dec 28 -> Jan 2 (cross month boundary, gap=5, saver was used)
        var userId = Guid.NewGuid();
        var db = TestDbContextFactory.Create();
        var vietnamNow = new DateTimeOffset(2027, 1, 2, 10, 0, 0, TimeSpan.Zero);
        var dec28 = new DateTimeOffset(2026, 12, 28, 10, 0, 0, TimeSpan.Zero);
        SeedStreak(db, userId, 5, 10, 4, dec28.DateTime, true); // Saver was used
        var sut = CreateSut(db, vietnamNow);

        // Act
        var result = await sut.Handle(new GetStreakQuery(userId), CancellationToken.None);

        // Assert
        result.IsCheckedInToday.Should().BeFalse();
        result.CurrentStreak.Should().Be(0);
        result.CycleDay.Should().Be(0);
        result.IsSaverUsed.Should().BeFalse(); // Saver reset for new month display (line 103)
    }
}