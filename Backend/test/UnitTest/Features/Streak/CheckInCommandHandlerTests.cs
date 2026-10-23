using FluentAssertions;
using Moq;
using MyTarotReader.Application.Common.Exceptions;
using MyTarotReader.Application.Common.Time;
using MyTarotReader.Application.Constants.Errors;
using MyTarotReader.Application.Features.Streak;
using MyTarotReader.Application.Services;
using MyTarotReader.Application.Settings;
using StreakEntity = MyTarotReader.Domain.Entities.Streak;
using MyTarotReader.Domain.Enums;
using MyTarotReader.Infrastructure.Persistence;
using MyTarotReader.UnitTest.Common;
using Xunit;

namespace MyTarotReader.UnitTest.Features.Streak;

public class CheckInCommandHandlerTests
{
    private static CheckInCommandHandler CreateSut(
        AppDbContext db,
        IWalletService? walletService = null,
        DateTimeOffset? fixedVietnamNow = null,
        StreakSetting? streakSetting = null)
    {
        var timeProviderMock = new Mock<ITimeProvider>();
        timeProviderMock
            .Setup(t => t.GetVietnamNow())
            .Returns(fixedVietnamNow ?? DateTimeOffset.UtcNow);

        var walletServiceMock = walletService ?? new Mock<IWalletService>().Object;

        var settings = streakSetting ?? new StreakSetting
        {
            CycleDays = 7,
            DailyCheckInRewards = new List<int> { 1, 1, 1, 1, 2, 2, 3 }
        };
        var optionsMock = new Mock<Microsoft.Extensions.Options.IOptions<StreakSetting>>();
        optionsMock.Setup(o => o.Value).Returns(settings);

        return new CheckInCommandHandler(db, timeProviderMock.Object, optionsMock.Object, walletServiceMock);
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
    public async Task Handle_NewUser_CreatesStreakAndGrantsReward()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var db = TestDbContextFactory.Create();
        var vietnamNow = new DateTimeOffset(2026, 10, 22, 10, 0, 0, TimeSpan.Zero);
        var walletServiceMock = new Mock<IWalletService>();
        var sut = CreateSut(db, walletServiceMock.Object, vietnamNow);

        // Act
        await sut.Handle(new CheckInCommand(userId), CancellationToken.None);

        // Assert
        var streak = db.Streaks.First();
        streak.CurrentStreak.Should().Be(1);
        streak.CycleDay.Should().Be(1);
        streak.LastCheckIn.Should().Be(vietnamNow.DateTime);
        streak.IsSaverUsed.Should().BeFalse();
        streak.LongestStreak.Should().Be(1);

        walletServiceMock.Verify(w => w.AddCoinsAsync(
            userId,
            0,
            1, // CycleDay 1 -> index 0 -> reward 1
            OrderType.DailyCheckIn,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Gap1_ConsecutiveDay_IncrementsStreakAndCycleDay()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var db = TestDbContextFactory.Create();
        var vietnamNow = new DateTimeOffset(2026, 10, 22, 10, 0, 0, TimeSpan.Zero);
        var yesterday = vietnamNow.AddDays(-1);
        SeedStreak(db, userId, 3, 5, 2, yesterday.DateTime, false);
        var walletServiceMock = new Mock<IWalletService>();
        var sut = CreateSut(db, walletServiceMock.Object, vietnamNow);

        // Act
        await sut.Handle(new CheckInCommand(userId), CancellationToken.None);

        // Assert
        var streak = db.Streaks.First();
        streak.CurrentStreak.Should().Be(4);
        streak.CycleDay.Should().Be(3);
        streak.LastCheckIn.Should().Be(vietnamNow.DateTime);
        streak.IsSaverUsed.Should().BeFalse();
        streak.LongestStreak.Should().Be(5);

        walletServiceMock.Verify(w => w.AddCoinsAsync(
            userId,
            0,
            1, // CycleDay 3 -> index 2 -> reward 1
            OrderType.DailyCheckIn,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Gap1_CycleDayWrapsTo1AfterCycleDays()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var db = TestDbContextFactory.Create();
        var vietnamNow = new DateTimeOffset(2026, 10, 22, 10, 0, 0, TimeSpan.Zero);
        var yesterday = vietnamNow.AddDays(-1);
        // CycleDay = 7 (last day of cycle), streak = 6
        SeedStreak(db, userId, 6, 10, 7, yesterday.DateTime, false);
        var walletServiceMock = new Mock<IWalletService>();
        var sut = CreateSut(db, walletServiceMock.Object, vietnamNow);

        // Act
        await sut.Handle(new CheckInCommand(userId), CancellationToken.None);

        // Assert
        var streak = db.Streaks.First();
        streak.CurrentStreak.Should().Be(7);
        streak.CycleDay.Should().Be(1); // Wraps to 1
        streak.LongestStreak.Should().Be(10);

        walletServiceMock.Verify(w => w.AddCoinsAsync(
            userId,
            0,
            1, // CycleDay 1 -> index 0 -> reward 1
            OrderType.DailyCheckIn,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Gap2_SameMonth_SaverUnused_StreakGreaterThan1_UsesSaver()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var db = TestDbContextFactory.Create();
        var vietnamNow = new DateTimeOffset(2026, 10, 22, 10, 0, 0, TimeSpan.Zero);
        var twoDaysAgo = vietnamNow.AddDays(-2);
        SeedStreak(db, userId, 3, 10, 2, twoDaysAgo.DateTime, false);
        var walletServiceMock = new Mock<IWalletService>();
        var sut = CreateSut(db, walletServiceMock.Object, vietnamNow);

        // Act
        await sut.Handle(new CheckInCommand(userId), CancellationToken.None);

        // Assert
        var streak = db.Streaks.First();
        streak.CurrentStreak.Should().Be(4); // Continued
        streak.CycleDay.Should().Be(3);
        streak.IsSaverUsed.Should().BeTrue(); // Saver now used

        walletServiceMock.Verify(w => w.AddCoinsAsync(
            userId,
            0,
            1, // CycleDay 3 -> index 2 -> reward 1
            OrderType.DailyCheckIn,
            It.IsAny<CancellationToken>()), Times.Once);
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
        var walletServiceMock = new Mock<IWalletService>();
        var sut = CreateSut(db, walletServiceMock.Object, vietnamNow);

        // Act
        await sut.Handle(new CheckInCommand(userId), CancellationToken.None);

        // Assert
        var streak = db.Streaks.First();
        streak.CurrentStreak.Should().Be(1); // Reset to 1
        streak.CycleDay.Should().Be(1);
        streak.IsSaverUsed.Should().BeFalse();

        walletServiceMock.Verify(w => w.AddCoinsAsync(
            userId,
            0,
            1, // CycleDay 1 -> index 0 -> reward 1
            OrderType.DailyCheckIn,
            It.IsAny<CancellationToken>()), Times.Once);
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
        var walletServiceMock = new Mock<IWalletService>();
        var sut = CreateSut(db, walletServiceMock.Object, vietnamNow);

        // Act
        await sut.Handle(new CheckInCommand(userId), CancellationToken.None);

        // Assert
        var streak = db.Streaks.First();
        streak.CurrentStreak.Should().Be(1);
        streak.CycleDay.Should().Be(1);
        streak.IsSaverUsed.Should().BeTrue();
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
        var walletServiceMock = new Mock<IWalletService>();
        var sut = CreateSut(db, walletServiceMock.Object, vietnamNow);

        // Act
        await sut.Handle(new CheckInCommand(userId), CancellationToken.None);

        // Assert
        var streak = db.Streaks.First();
        streak.CurrentStreak.Should().Be(1);
        streak.CycleDay.Should().Be(1);
        streak.IsSaverUsed.Should().BeFalse(); // Saver resets on new month
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
        var walletServiceMock = new Mock<IWalletService>();
        var sut = CreateSut(db, walletServiceMock.Object, vietnamNow);

        // Act
        await sut.Handle(new CheckInCommand(userId), CancellationToken.None);

        // Assert
        var streak = db.Streaks.First();
        streak.CurrentStreak.Should().Be(1);
        streak.CycleDay.Should().Be(1);
        streak.IsSaverUsed.Should().BeFalse(); // Saver resets on new month (line 133)
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
        var walletServiceMock = new Mock<IWalletService>();
        var sut = CreateSut(db, walletServiceMock.Object, vietnamNow);

        // Act
        await sut.Handle(new CheckInCommand(userId), CancellationToken.None);

        // Assert
        var streak = db.Streaks.First();
        streak.CurrentStreak.Should().Be(1);
        streak.CycleDay.Should().Be(1);
        streak.IsSaverUsed.Should().BeTrue(); // Saver preserved in same month
    }

    [Fact]
    public async Task Handle_AlreadyCheckedInToday_ThrowsBadRequestException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var db = TestDbContextFactory.Create();
        var vietnamNow = new DateTimeOffset(2026, 10, 22, 10, 0, 0, TimeSpan.Zero);
        SeedStreak(db, userId, 5, 10, 4, vietnamNow.DateTime, false);
        var walletServiceMock = new Mock<IWalletService>();
        var sut = CreateSut(db, walletServiceMock.Object, vietnamNow);

        // Act & Assert
        var act = () => sut.Handle(new CheckInCommand(userId), CancellationToken.None);
        await act.Should().ThrowAsync<BadRequestException>()
            .Where(e => e.ErrorCode == StreakErrorCode.AlreadyCheckedIn);

        // Wallet service should not be called
        walletServiceMock.Verify(w => w.AddCoinsAsync(
            It.IsAny<Guid>(),
            It.IsAny<int>(),
            It.IsAny<int>(),
            It.IsAny<OrderType>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_UpdatesLongestStreak_WhenCurrentExceedsLongest()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var db = TestDbContextFactory.Create();
        var vietnamNow = new DateTimeOffset(2026, 10, 22, 10, 0, 0, TimeSpan.Zero);
        var yesterday = vietnamNow.AddDays(-1);
        // Current streak = 10, Longest = 8
        SeedStreak(db, userId, 10, 8, 3, yesterday.DateTime, false);
        var walletServiceMock = new Mock<IWalletService>();
        var sut = CreateSut(db, walletServiceMock.Object, vietnamNow);

        // Act
        await sut.Handle(new CheckInCommand(userId), CancellationToken.None);

        // Assert
        var streak = db.Streaks.First();
        streak.CurrentStreak.Should().Be(11);
        streak.LongestStreak.Should().Be(11); // Updated to new current streak
    }

    [Fact]
    public async Task Handle_RewardCalculation_MatchesDailyCheckInRewards()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var db = TestDbContextFactory.Create();
        var vietnamNow = new DateTimeOffset(2026, 10, 22, 10, 0, 0, TimeSpan.Zero);
        var yesterday = vietnamNow.AddDays(-1);
        
        var customSetting = new StreakSetting
        {
            CycleDays = 7,
            DailyCheckInRewards = new List<int> { 1, 1, 1, 1, 2, 2, 3 }
        };
        
        var walletServiceMock = new Mock<IWalletService>();
        var sut = CreateSut(db, walletServiceMock.Object, vietnamNow, customSetting);

        // Test each cycle day reward
        var testCases = new[]
        {
            new { CycleDay = 1, ExpectedReward = 1 }, // index 0
            new { CycleDay = 2, ExpectedReward = 1 }, // index 1
            new { CycleDay = 3, ExpectedReward = 1 }, // index 2
            new { CycleDay = 4, ExpectedReward = 1 }, // index 3
            new { CycleDay = 5, ExpectedReward = 2 }, // index 4
            new { CycleDay = 6, ExpectedReward = 2 }, // index 5
            new { CycleDay = 7, ExpectedReward = 3 }, // index 6
            new { CycleDay = 8, ExpectedReward = 1 }, // wraps to index 0 (but cycle day wraps to 1)
        };

        foreach (var tc in testCases)
        {
            // Create fresh db for each test case
            var testDb = TestDbContextFactory.Create();
            var testWalletMock = new Mock<IWalletService>();
            var testSut = CreateSut(testDb, testWalletMock.Object, vietnamNow, customSetting);
            
            var testUserId = Guid.NewGuid();
            var testYesterday = vietnamNow.AddDays(-1);
            SeedStreak(testDb, testUserId, tc.CycleDay - 1, tc.CycleDay - 1, tc.CycleDay - 1, testYesterday.DateTime, false);
            
            await testSut.Handle(new CheckInCommand(testUserId), CancellationToken.None);
            
            testWalletMock.Verify(w => w.AddCoinsAsync(
                testUserId,
                0,
                tc.ExpectedReward,
                OrderType.DailyCheckIn,
                It.IsAny<CancellationToken>()), Times.Once);
        }
    }

    [Fact]
    public async Task Handle_Gap1_CrossMonth_ResetsSaverToFalse()
    {
        // Arrange - Dec 31 -> Jan 1 (cross month boundary, consecutive day)
        var userId = Guid.NewGuid();
        var db = TestDbContextFactory.Create();
        var vietnamNow = new DateTimeOffset(2027, 1, 1, 10, 0, 0, TimeSpan.Zero);
        var dec31 = new DateTimeOffset(2026, 12, 31, 10, 0, 0, TimeSpan.Zero);
        SeedStreak(db, userId, 5, 10, 4, dec31.DateTime, true); // Saver was used last month
        var walletServiceMock = new Mock<IWalletService>();
        var sut = CreateSut(db, walletServiceMock.Object, vietnamNow);

        // Act
        await sut.Handle(new CheckInCommand(userId), CancellationToken.None);

        // Assert
        var streak = db.Streaks.First();
        streak.CurrentStreak.Should().Be(6); // Continued
        streak.CycleDay.Should().Be(5);
        streak.IsSaverUsed.Should().BeFalse(); // Saver reset for new month
        streak.LongestStreak.Should().Be(10);

        walletServiceMock.Verify(w => w.AddCoinsAsync(
            userId,
            0,
            2, // CycleDay 5 -> index 4 -> reward 2
            OrderType.DailyCheckIn,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Gap2_CrossMonth_SaverAlreadyUsed_ResetsStreakAndResetsSaver()
    {
        // Arrange - Dec 30 -> Jan 1 (cross month boundary, gap=2, saver already used)
        var userId = Guid.NewGuid();
        var db = TestDbContextFactory.Create();
        var vietnamNow = new DateTimeOffset(2027, 1, 1, 10, 0, 0, TimeSpan.Zero);
        var dec30 = new DateTimeOffset(2026, 12, 30, 10, 0, 0, TimeSpan.Zero);
        SeedStreak(db, userId, 5, 10, 4, dec30.DateTime, true); // Saver already used
        var walletServiceMock = new Mock<IWalletService>();
        var sut = CreateSut(db, walletServiceMock.Object, vietnamNow);

        // Act
        await sut.Handle(new CheckInCommand(userId), CancellationToken.None);

        // Assert
        var streak = db.Streaks.First();
        streak.CurrentStreak.Should().Be(1); // Reset
        streak.CycleDay.Should().Be(1);
        streak.IsSaverUsed.Should().BeFalse(); // Saver resets on new month
    }

    [Fact]
    public async Task Handle_Gap2_CrossMonth_StreakOneOrLess_SaverUnused_ResetsStreakAndResetsSaver()
    {
        // Arrange - Dec 30 -> Jan 1 (cross month boundary, gap=2, streak=1, saver unused)
        var userId = Guid.NewGuid();
        var db = TestDbContextFactory.Create();
        var vietnamNow = new DateTimeOffset(2027, 1, 1, 10, 0, 0, TimeSpan.Zero);
        var dec30 = new DateTimeOffset(2026, 12, 30, 10, 0, 0, TimeSpan.Zero);
        SeedStreak(db, userId, 1, 10, 0, dec30.DateTime, false); // Streak <= 1, saver unused
        var walletServiceMock = new Mock<IWalletService>();
        var sut = CreateSut(db, walletServiceMock.Object, vietnamNow);

        // Act
        await sut.Handle(new CheckInCommand(userId), CancellationToken.None);

        // Assert
        var streak = db.Streaks.First();
        streak.CurrentStreak.Should().Be(1); // Reset to 1
        streak.CycleDay.Should().Be(1);
        streak.IsSaverUsed.Should().BeFalse(); // Saver reset for new month (not consumed)

        walletServiceMock.Verify(w => w.AddCoinsAsync(
            userId,
            0,
            1, // CycleDay 1 -> index 0 -> reward 1
            OrderType.DailyCheckIn,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Gap3OrMore_NewMonth_SaverPreviouslyUsed_ResetsSaverToFalse()
    {
        // Arrange - Oct 18 -> Oct 22 (same month, gap=4) but test new month scenario
        // Actually need a cross-month gap>=3: e.g., Dec 28 -> Jan 2 (gap=5, new month)
        var userId = Guid.NewGuid();
        var db = TestDbContextFactory.Create();
        var vietnamNow = new DateTimeOffset(2027, 1, 2, 10, 0, 0, TimeSpan.Zero);
        var dec28 = new DateTimeOffset(2026, 12, 28, 10, 0, 0, TimeSpan.Zero);
        SeedStreak(db, userId, 5, 10, 4, dec28.DateTime, true); // Saver was used
        var walletServiceMock = new Mock<IWalletService>();
        var sut = CreateSut(db, walletServiceMock.Object, vietnamNow);

        // Act
        await sut.Handle(new CheckInCommand(userId), CancellationToken.None);

        // Assert
        var streak = db.Streaks.First();
        streak.CurrentStreak.Should().Be(1); // Reset
        streak.CycleDay.Should().Be(1);
        streak.IsSaverUsed.Should().BeFalse(); // Saver reset for new month (line 133)
    }
}

