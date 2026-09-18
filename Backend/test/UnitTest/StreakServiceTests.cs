using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Moq;
using MyTarotReader.Application.Common.Exceptions;
using MyTarotReader.Application.Constants.Errors;
using MyTarotReader.Application.Contracts.Services;
using MyTarotReader.Domain.Common;
using MyTarotReader.Domain.Entities;
using MyTarotReader.Domain.Enums;
using MyTarotReader.Infrastructure.Persistence;
using MyTarotReader.Infrastructure.Services;
using Xunit;

namespace MyTarotReader.UnitTest;

/// <summary>
/// Unit tests for <see cref="StreakService"/>, running against a real
/// <see cref="AppDbContext"/> with the EF Core InMemory provider. The InMemory provider
/// does not support real transactions, so the transactional warning
/// (<see cref="InMemoryEventId.TransactionIgnoredWarning"/>) is suppressed to let
/// <c>BeginTransactionAsync</c> act as a no-op; the reward flow is verified by asserting
/// the calls made to the mocked <see cref="IWalletService"/>.
/// </summary>
public class StreakServiceTests
{
    #region Helpers

    private static AppDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .EnableServiceProviderCaching(false)
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        return new AppDbContext(options);
    }

    private static (
        StreakService Service,
        AppDbContext Db,
        Mock<IWalletService> Wallet
    ) CreateSut()
    {
        var db = CreateInMemoryContext();
        var wallet = new Mock<IWalletService>();
        wallet
            .Setup(w =>
                w.AddCoinAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<AddCoinRequest>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(new AddCoinResult(0, 0));
        var service = new StreakService(db, wallet.Object);
        return (service, db, wallet);
    }

    private static async Task<Streak> SeedStreakAsync(
        AppDbContext db,
        Guid userId,
        DateTime lastCheckIn,
        int current = 1,
        int longest = 1,
        int cycleDay = 1,
        bool saver = false
    )
    {
        var streak = new Streak
        {
            UserId = userId,
            LastCheckIn = lastCheckIn,
            CurrentStreak = current,
            LongestStreak = longest,
            CycleDay = cycleDay,
            IsSaverUsed = saver,
        };
        db.Streaks.Add(streak);
        await db.SaveChangesAsync();
        return streak;
    }

    private static DateTime VietnamNow() => StreakHelper.GetVietnamNow();

    #endregion

    #region CheckInAsync

    /// <summary>
    /// First check-in with no existing streak creates one (CurrentStreak=1, LongestStreak=1,
    /// CycleDay=1, LastCheckIn=now VN) and grants the day-1 white coin reward via
    /// <see cref="IWalletService.AddCoinAsync"/>.
    /// </summary>
    [Fact]
    public async Task CheckIn_NoStreak_CreatesStreakAndRewardsFirstDay()
    {
        var (service, db, wallet) = CreateSut();
        var userId = Guid.NewGuid();

        var result = await service.CheckInAsync(userId);

        var streak = Assert.Single(db.Streaks);
        streak.UserId.Should().Be(userId);
        streak.CurrentStreak.Should().Be(1);
        streak.LongestStreak.Should().Be(1);
        streak.CycleDay.Should().Be(1);
        streak.LastCheckIn.Should().BeCloseTo(VietnamNow(), TimeSpan.FromMinutes(1));

        result.CurrentStreak.Should().Be(1);
        result.LongestStreak.Should().Be(1);
        result.CycleDay.Should().Be(1);
        result.IsSaverUsed.Should().BeFalse();
        result.IsCheckedInToday.Should().BeTrue();

        wallet.Verify(
            w =>
                w.AddCoinAsync(
                    userId,
                    It.Is<AddCoinRequest>(r =>
                        r.WhiteCoins == 1 && r.RedCoins == 0 && r.Type == OrderType.DailyCheckIn
                    ),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    /// <summary>
    /// Checking in twice on the same day (VN) throws <see cref="BadRequestException"/> with the
    /// <c>alreadyCheckedIn</c> code and no reward is granted.
    /// </summary>
    [Fact]
    public async Task CheckIn_AlreadyCheckedInToday_ThrowsBadRequest()
    {
        var (service, db, wallet) = CreateSut();
        var userId = Guid.NewGuid();
        await SeedStreakAsync(db, userId, VietnamNow());

        var act = async () => await service.CheckInAsync(userId);

        await act.Should()
            .ThrowAsync<BadRequestException>()
            .Where(e => e.ErrorCode == StreakErrorCode.AlreadyCheckedIn);
        wallet.Verify(
            w =>
                w.AddCoinAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<AddCoinRequest>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Never
        );
    }

    /// <summary>
    /// A consecutive check-in (last check-in was yesterday VN) increments the streak and the
    /// cycle day, and rewards the next white coin amount in the sequence.
    /// </summary>
    [Fact]
    public async Task CheckIn_ConsecutiveDay_IncrementsStreakAndCycle()
    {
        var (service, db, wallet) = CreateSut();
        var userId = Guid.NewGuid();
        await SeedStreakAsync(db, userId, VietnamNow().AddDays(-1), current: 1, longest: 1, cycleDay: 1);

        var result = await service.CheckInAsync(userId);

        var streak = db.Streaks.Single();
        streak.CurrentStreak.Should().Be(2);
        streak.LongestStreak.Should().Be(2);
        streak.CycleDay.Should().Be(2);

        result.CycleDay.Should().Be(2);
        result.CurrentStreak.Should().Be(2);

        wallet.Verify(
            w =>
                w.AddCoinAsync(
                    userId,
                    It.Is<AddCoinRequest>(r =>
                        r.WhiteCoins == 1 && r.RedCoins == 0 && r.Type == OrderType.DailyCheckIn
                    ),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    /// <summary>
    /// The fifth consecutive check-in rewards 2 white coins (<c>[1,1,1,1,2,2,3]</c>), matching
    /// the sequence driven by <c>(CurrentStreak - 1) % 7</c>.
    /// </summary>
    [Fact]
    public async Task CheckIn_FifthConsecutiveDay_RewardsTwoWhiteCoins()
    {
        var (service, db, wallet) = CreateSut();
        var userId = Guid.NewGuid();
        await SeedStreakAsync(db, userId, VietnamNow().AddDays(-1), current: 4, longest: 4, cycleDay: 4);

        var result = await service.CheckInAsync(userId);

        result.CurrentStreak.Should().Be(5);
        result.CycleDay.Should().Be(5);

        wallet.Verify(
            w =>
                w.AddCoinAsync(
                    userId,
                    It.Is<AddCoinRequest>(r =>
                        r.WhiteCoins == 2 && r.RedCoins == 0 && r.Type == OrderType.DailyCheckIn
                    ),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    /// <summary>
    /// Completing a 7-day cycle wraps the stored cycle day back to 1 (a check-in "on day 7"
    /// resets to 1) and the last day of the cycle rewards the maximum 3 white coins.
    /// </summary>
    [Fact]
    public async Task CheckIn_CompletesCycle_WrapsDayAndRewardsMax()
    {
        var (service, db, wallet) = CreateSut();
        var userId = Guid.NewGuid();
        await SeedStreakAsync(db, userId, VietnamNow().AddDays(-1), current: 6, longest: 6, cycleDay: 6);

        var result = await service.CheckInAsync(userId);

        var streak = db.Streaks.Single();
        streak.CurrentStreak.Should().Be(7);
        streak.CycleDay.Should().Be(1);
        result.CycleDay.Should().Be(1);

        wallet.Verify(
            w =>
                w.AddCoinAsync(
                    userId,
                    It.Is<AddCoinRequest>(r =>
                        r.WhiteCoins == 3 && r.RedCoins == 0 && r.Type == OrderType.DailyCheckIn
                    ),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    /// <summary>
    /// A check-in after missing more than one day resets the streak and cycle day to 1,
    /// keeps the historical LongestStreak, and rewards the day-1 amount.
    /// </summary>
    [Fact]
    public async Task CheckIn_AfterMissedDays_ResetsStreak()
    {
        var (service, db, wallet) = CreateSut();
        var userId = Guid.NewGuid();
        await SeedStreakAsync(db, userId, VietnamNow().AddDays(-3), current: 4, longest: 9, cycleDay: 4);

        var result = await service.CheckInAsync(userId);

        var streak = db.Streaks.Single();
        streak.CurrentStreak.Should().Be(1);
        streak.LongestStreak.Should().Be(9);
        streak.CycleDay.Should().Be(1);

        result.CurrentStreak.Should().Be(1);
        result.LongestStreak.Should().Be(9);

        wallet.Verify(
            w =>
                w.AddCoinAsync(
                    userId,
                    It.Is<AddCoinRequest>(r =>
                        r.WhiteCoins == 1 && r.RedCoins == 0 && r.Type == OrderType.DailyCheckIn
                    ),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    #endregion

    #region GetStreakAsync

    /// <summary>
    /// A user without a streak gets the default (sample) values: all zeros and false.
    /// </summary>
    [Fact]
    public async Task GetStreak_NoStreak_ReturnsDefaultValues()
    {
        var (service, _, _) = CreateSut();

        var result = await service.GetStreakAsync(Guid.NewGuid());

        result.CycleDay.Should().Be(0);
        result.CurrentStreak.Should().Be(0);
        result.LongestStreak.Should().Be(0);
        result.IsSaverUsed.Should().BeFalse();
        result.IsCheckedInToday.Should().BeFalse();
    }

    /// <summary>
    /// A user who checked in today sees their streak values, saver status and
    /// IsCheckedInToday=true.
    /// </summary>
    [Fact]
    public async Task GetStreak_CheckedInToday_ReturnsValues()
    {
        var (service, db, _) = CreateSut();
        var userId = Guid.NewGuid();
        await SeedStreakAsync(db, userId, VietnamNow(), current: 5, longest: 8, cycleDay: 5);

        var result = await service.GetStreakAsync(userId);

        result.CycleDay.Should().Be(5);
        result.CurrentStreak.Should().Be(5);
        result.LongestStreak.Should().Be(8);
        result.IsSaverUsed.Should().BeFalse();
        result.IsCheckedInToday.Should().BeTrue();
    }

    /// <summary>
    /// A stored cycle day of 7 is displayed as 0 (the cycle runs 0-6).
    /// </summary>
    [Fact]
    public async Task GetStreak_CycleDaySeven_ShowsZero()
    {
        var (service, db, _) = CreateSut();
        var userId = Guid.NewGuid();
        await SeedStreakAsync(db, userId, VietnamNow(), current: 7, longest: 7, cycleDay: 7);

        var result = await service.GetStreakAsync(userId);

        result.CycleDay.Should().Be(0);
    }

    /// <summary>
    /// After missing more than one day, GET reports (and persists) a reset streak:
    /// CurrentStreak and CycleDay drop back to 0 while LongestStreak is kept.
    /// </summary>
    [Fact]
    public async Task GetStreak_BrokenStreak_ResetsToZero()
    {
        var (service, db, _) = CreateSut();
        var userId = Guid.NewGuid();
        await SeedStreakAsync(db, userId, VietnamNow().AddDays(-3), current: 5, longest: 8, cycleDay: 5);

        var result = await service.GetStreakAsync(userId);

        result.CurrentStreak.Should().Be(0);
        result.CycleDay.Should().Be(0);
        result.LongestStreak.Should().Be(8);
        result.IsCheckedInToday.Should().BeFalse();

        var persisted = db.Streaks.Single();
        persisted.CurrentStreak.Should().Be(0);
        persisted.CycleDay.Should().Be(0);
    }

    /// <summary>
    /// The streak saver is automatically reset to false when a new calendar month (VN time)
    /// has started since the last check-in, and the change is persisted.
    /// </summary>
    [Fact]
    public async Task GetStreak_NewMonth_ResetsSaver()
    {
        var (service, db, _) = CreateSut();
        var userId = Guid.NewGuid();
        await SeedStreakAsync(
            db,
            userId,
            VietnamNow().AddMonths(-1),
            current: 0,
            longest: 10,
            cycleDay: 0,
            saver: true
        );

        var result = await service.GetStreakAsync(userId);

        result.IsSaverUsed.Should().BeFalse();
        db.Streaks.Single().IsSaverUsed.Should().BeFalse();
    }

    #endregion
}