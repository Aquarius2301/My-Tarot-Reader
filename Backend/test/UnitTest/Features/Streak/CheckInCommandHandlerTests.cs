using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using MyTarotReader.Application.Common.Exceptions;
using MyTarotReader.Application.Common.Helpers;
using MyTarotReader.Application.Constants.Errors;
using MyTarotReader.Application.Features.Streak;
using MyTarotReader.Application.Services;
using MyTarotReader.Application.Settings;
using MyTarotReader.Domain.Enums;
using MyTarotReader.Infrastructure.Persistence;
using MyTarotReader.UnitTest.Common;
using Xunit;
using StreakEntity = MyTarotReader.Domain.Entities.Streak;

namespace MyTarotReader.UnitTest.Features.Streak;

public class CheckInCommandHandlerTests
{
    private static (
        CheckInCommandHandler Sut,
        AppDbContext Db,
        Mock<IWalletService> Wallet
    ) CreateSut()
    {
        var db = TestDbContextFactory.Create();
        var wallet = new Mock<IWalletService>();
        wallet
            .Setup(x =>
                x.AddCoinsAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<OrderType>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .Returns(Task.CompletedTask);

        var sut = new CheckInCommandHandler(
            db,
            wallet.Object,
            Options.Create(new StreakSetting())
        );

        return (sut, db, wallet);
    }

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
    public async Task CheckIn_NoStreak_CreatesStreakAndRewardsFirstDay()
    {
        // Arrange
        var (sut, db, wallet) = CreateSut();
        var userId = Guid.NewGuid();

        // Act
        await sut.Handle(new CheckInCommand(userId), CancellationToken.None);

        // Assert
        var streak = db.Streaks.Should().ContainSingle().Subject;
        streak.UserId.Should().Be(userId);
        streak.CurrentStreak.Should().Be(1);
        streak.LongestStreak.Should().Be(1);
        streak.CycleDay.Should().Be(1);
        streak.LastCheckIn.Should().BeCloseTo(StreakHelper.GetVietnamNow(), TimeSpan.FromMinutes(1));
        wallet.Verify(
            x =>
                x.AddCoinsAsync(
                    userId,
                    0,
                    1,
                    OrderType.DailyCheckIn,
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task CheckIn_AlreadyCheckedInToday_ThrowsBadRequestAndRewardsNothing()
    {
        // Arrange
        var (sut, db, wallet) = CreateSut();
        var userId = Guid.NewGuid();
        SeedStreak(db, userId, StreakHelper.GetVietnamNow());

        // Act
        var act = () => sut.Handle(new CheckInCommand(userId), CancellationToken.None);

        // Assert
        var ex = await act.Should().ThrowAsync<BadRequestException>();
        ex.Which.ErrorCode.Should().Be(StreakErrorCode.AlreadyCheckedIn);
        wallet.Verify(
            x =>
                x.AddCoinsAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<OrderType>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Never
        );
    }

    [Fact]
    public async Task CheckIn_ConsecutiveDay_IncrementsStreakAndCycle()
    {
        // Arrange
        var (sut, db, wallet) = CreateSut();
        var userId = Guid.NewGuid();
        var streak = SeedStreak(db, userId, StreakHelper.GetVietnamNow().AddDays(-1));

        // Act
        await sut.Handle(new CheckInCommand(userId), CancellationToken.None);

        // Assert
        streak.CurrentStreak.Should().Be(2);
        streak.CycleDay.Should().Be(2);
        wallet.Verify(
            x =>
                x.AddCoinsAsync(
                    userId,
                    0,
                    1,
                    OrderType.DailyCheckIn,
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task CheckIn_FifthConsecutiveDay_RewardsTwoWhiteCoins()
    {
        // Arrange
        var (sut, db, wallet) = CreateSut();
        var userId = Guid.NewGuid();
        SeedStreak(
            db,
            userId,
            StreakHelper.GetVietnamNow().AddDays(-1),
            current: 4,
            cycleDay: 4
        );

        // Act
        await sut.Handle(new CheckInCommand(userId), CancellationToken.None);

        // Assert
        wallet.Verify(
            x =>
                x.AddCoinsAsync(
                    userId,
                    0,
                    2,
                    OrderType.DailyCheckIn,
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task CheckIn_CompletesCycle_WrapsDayAndRewardsMax()
    {
        // Arrange
        var (sut, db, wallet) = CreateSut();
        var userId = Guid.NewGuid();
        var streak = SeedStreak(
            db,
            userId,
            StreakHelper.GetVietnamNow().AddDays(-1),
            current: 6,
            cycleDay: 6
        );

        // Act
        await sut.Handle(new CheckInCommand(userId), CancellationToken.None);

        // Assert
        streak.CurrentStreak.Should().Be(7);
        streak.CycleDay.Should().Be(1);
        wallet.Verify(
            x =>
                x.AddCoinsAsync(
                    userId,
                    0,
                    3,
                    OrderType.DailyCheckIn,
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task CheckIn_AfterMissedDays_ResetsStreakAndKeepsLongest()
    {
        // Arrange
        var (sut, db, wallet) = CreateSut();
        var userId = Guid.NewGuid();
        var streak = SeedStreak(
            db,
            userId,
            StreakHelper.GetVietnamNow().AddDays(-3),
            current: 5,
            longest: 9,
            cycleDay: 3
        );

        // Act
        await sut.Handle(new CheckInCommand(userId), CancellationToken.None);

        // Assert
        streak.CurrentStreak.Should().Be(1);
        streak.CycleDay.Should().Be(1);
        streak.LongestStreak.Should().Be(9);
        streak.IsSaverUsed.Should().BeTrue();
        wallet.Verify(
            x =>
                x.AddCoinsAsync(
                    userId,
                    0,
                    1,
                    OrderType.DailyCheckIn,
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task CheckIn_AfterMissedDays_NewMonth_ResetsSaver()
    {
        // Arrange
        var (sut, db, _) = CreateSut();
        var userId = Guid.NewGuid();
        var now = StreakHelper.GetVietnamNow();
        var lastMonth = new DateTime(now.Year, now.Month, 1).AddMonths(-1);
        var streak = SeedStreak(db, userId, lastMonth, current: 5, saver: true);

        // Act
        await sut.Handle(new CheckInCommand(userId), CancellationToken.None);

        // Assert
        streak.IsSaverUsed.Should().BeFalse();
    }

    [Fact]
    public async Task CheckIn_AfterMissedDays_UpdatesLongestStreakWhenZero()
    {
        // Arrange
        var (sut, db, _) = CreateSut();
        var userId = Guid.NewGuid();
        var streak = SeedStreak(
            db,
            userId,
            StreakHelper.GetVietnamNow().AddDays(-3),
            current: 0,
            longest: 0,
            cycleDay: 0
        );

        // Act
        await sut.Handle(new CheckInCommand(userId), CancellationToken.None);

        // Assert
        streak.CurrentStreak.Should().Be(1);
        streak.LongestStreak.Should().Be(1);
    }
}
