using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MyTarotReader.Application.Common.Exceptions;
using MyTarotReader.Application.Common.Helpers;
using MyTarotReader.Application.Constants.Errors;
using MyTarotReader.Application.Contracts.Persistence;
using MyTarotReader.Application.Contracts.Services;
using MyTarotReader.Application.Settings;
using MyTarotReader.Domain.Common;
using MyTarotReader.Domain.Entities;
using MyTarotReader.Domain.Enums;

namespace MyTarotReader.Infrastructure.Services;

public class StreakService(
    IAppDbContext context,
    IWalletService walletService,
    IOptions<StreakSetting> streakSetting
) : IStreakService
{
    private readonly IAppDbContext _context = context;
    private readonly IWalletService _walletService = walletService;
    private readonly StreakSetting _streakSetting = streakSetting.Value;

    /// <summary>
    /// Retrieves the current streak information for a user. When the streak is broken or a
    /// new month has started, the persisted state is reset before the result is returned.
    /// </summary>
    /// <param name="userId">The authenticated user's ID.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>
    /// <see cref="GetStreakResult"/> containing the cycle day, current/longest streak,
    /// saver status and whether the user checked in today. Returns default values
    /// when the user has no streak yet.
    /// </returns>
    public async Task<GetStreakResult> GetStreakAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    )
    {
        var streak = await _context.Streaks.FirstOrDefaultAsync(
            s => s.UserId == userId,
            cancellationToken
        );

        if (streak is null)
        {
            return new GetStreakResult(0, 0, 0, false, false);
        }

        var vietnamNow = StreakHelper.GetVietnamNow();
        var resetSaver = ResetSaverIfNewMonth(streak, vietnamNow);
        var resetStreak = StreakHelper.IsBroken(streak.LastCheckIn, vietnamNow);
        if (resetStreak)
        {
            streak.CurrentStreak = 0;
            streak.CycleDay = 0;
        }

        if (resetSaver || resetStreak)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }

        return new GetStreakResult(
            GetCycleDayToShow(streak.CycleDay),
            streak.CurrentStreak,
            streak.LongestStreak,
            streak.IsSaverUsed,
            StreakHelper.IsSameDay(streak.LastCheckIn, vietnamNow)
        );
    }

    /// <summary>
    /// Performs a daily check-in for a user, creating a new streak when none exists, and
    /// grants the corresponding white coin reward.
    /// </summary>
    /// <param name="userId">The authenticated user's ID.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <exception cref="BadRequestException">
    /// Thrown when the user has already checked in today.
    /// </exception>
    public async Task CheckInAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var vietnamNow = StreakHelper.GetVietnamNow();

        await using var transaction = await _context.Database.BeginTransactionAsync(
            cancellationToken
        );

        var streak = await _context.Streaks.FirstOrDefaultAsync(
            s => s.UserId == userId,
            cancellationToken
        );

        if (streak is null)
        {
            streak = new Streak
            {
                UserId = userId,
                CurrentStreak = 1,
                LongestStreak = 1,
                CycleDay = 1,
                LastCheckIn = vietnamNow,
            };
            _context.Streaks.Add(streak);
        }
        else
        {
            ResetSaverIfNewMonth(streak, vietnamNow);

            if (StreakHelper.IsSameDay(streak.LastCheckIn, vietnamNow))
            {
                throw new BadRequestException(StreakErrorCode.AlreadyCheckedIn);
            }

            if (StreakHelper.IsConsecutiveDay(streak.LastCheckIn, vietnamNow))
            {
                streak.CurrentStreak++;
                streak.CycleDay = GetNextCycleDay(streak.CycleDay);
            }
            else
            {
                streak.CurrentStreak = 1;
                streak.CycleDay = 1;
            }

            streak.LongestStreak = Math.Max(streak.LongestStreak, streak.CurrentStreak);
            streak.LastCheckIn = vietnamNow;
        }

        await _context.SaveChangesAsync(cancellationToken);

        var rewards = _streakSetting.DailyCheckInRewards;
        var reward = rewards[(streak.CurrentStreak - 1) % rewards.Count];
        await _walletService.AddCoinAsync(
            userId,
            new AddCoinRequest(reward, 0, OrderType.DailyCheckIn),
            cancellationToken
        );

        await transaction.CommitAsync(cancellationToken);
    }

    /// <summary>
    /// Resets the streak saver flag when a new calendar month (Vietnam time) has
    /// started since the last check-in.
    /// </summary>
    private static bool ResetSaverIfNewMonth(Streak streak, DateTime vietnamNow)
    {
        if (!streak.IsSaverUsed || !StreakHelper.IsNewMonth(streak.LastCheckIn, vietnamNow))
        {
            return false;
        }

        streak.IsSaverUsed = false;
        return true;
    }

    /// <summary>
    /// Normalizes the cycle day for display: a 7-day cycle runs from 0 to 6,
    /// so a stored value of 7 is shown as 0.
    /// </summary>
    private int GetCycleDayToShow(int cycleDay) => cycleDay % _streakSetting.CycleDays;

    /// <summary>
    /// Computes the next cycle day after a consecutive check-in. When the cycle
    /// would complete (value 7), it restarts from 1 instead.
    /// </summary>
    private int GetNextCycleDay(int cycleDay)
    {
        var next = cycleDay + 1;
        return next >= _streakSetting.CycleDays ? 1 : next;
    }
}
