using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MyTarotReader.Application.Common.Exceptions;
using MyTarotReader.Application.Common.Time;
using MyTarotReader.Application.Constants.Errors;
using MyTarotReader.Application.Persistence;
using MyTarotReader.Application.Services;
using MyTarotReader.Application.Settings;
using MyTarotReader.Domain.Enums;
using StreakEntity = MyTarotReader.Domain.Entities.Streak;

namespace MyTarotReader.Application.Features.Streak;

public class CheckInCommandHandler(
    IAppDbContext context,
    ITimeProvider timeProvider,
    IOptions<StreakSetting> streakSetting,
    IWalletService walletService
) : IRequestHandler<CheckInCommand, Unit>
{
    private readonly StreakSetting _streakSetting = streakSetting.Value;

    public async Task<Unit> Handle(CheckInCommand request, CancellationToken cancellationToken)
    {
        var now = timeProvider.GetVietnamNow();
        var today = now.DateTime.Date;

        var streak = await context.Streaks.FirstOrDefaultAsync(
            s => s.UserId == request.UserId,
            cancellationToken
        );

        if (streak == null) // Create new streak for user if empty
        {
            streak = new StreakEntity
            {
                UserId = request.UserId,
                CurrentStreak = 0,
                LongestStreak = 0,
                CycleDay = 0,
                LastCheckIn = DateTime.MinValue,
                IsSaverUsed = false,
            };
            context.Streaks.Add(streak);
        }

        // Check if already checked in today
        if (streak.LastCheckIn != DateTime.MinValue && streak.LastCheckIn.Date == today)
        {
            throw new BadRequestException(StreakErrorCode.AlreadyCheckedIn);
        }

        var lastCheckInDate =
            streak.LastCheckIn == DateTime.MinValue ? today.AddDays(-100) : streak.LastCheckIn.Date;

        var gap = (today - lastCheckInDate).Days;

        // Determine if it's a new month based on the last check-in date
        bool isNewMonth =
            lastCheckInDate.Month != today.Month || lastCheckInDate.Year != today.Year;

        int newCurrentStreak;
        int newCycleDay;
        bool newIsSaverUsed;

        if (streak.LastCheckIn == DateTime.MinValue)
        {
            // New user
            newCurrentStreak = 1;
            newCycleDay = 1;
            newIsSaverUsed = false;
        }
        else if (gap == 1)
        {
            // Consecutive day
            newCurrentStreak = streak.CurrentStreak + 1;
            newCycleDay = streak.CycleDay + 1;
            if (newCycleDay > _streakSetting.CycleDays)
            {
                newCycleDay = 1;
            }
            // Preserve actual saver state (reset to false if crossing month boundary via gap=1)
            newIsSaverUsed = isNewMonth ? false : streak.IsSaverUsed;
        }
        else if (gap == 2)
        {
            if (isNewMonth)
            {
                // Cross-month boundary (missed day from previous month to new month)
                newCurrentStreak = 1;
                newCycleDay = 1;
                newIsSaverUsed = false; // FIX: New month means saver ticket must be UNUSED (false)
            }
            else
            {
                // Same month
                if (!streak.IsSaverUsed)
                {
                    if (streak.CurrentStreak > 1)
                    {
                        // Use saver
                        newCurrentStreak = streak.CurrentStreak + 1;
                        newCycleDay = streak.CycleDay + 1;
                        if (newCycleDay > _streakSetting.CycleDays)
                        {
                            newCycleDay = 1;
                        }
                        newIsSaverUsed = true; // Saver used to successfully save streak
                    }
                    else
                    {
                        // No streak to save (<= 1) -> Don't rescue, reset streak, keep saver unused
                        newCurrentStreak = 1;
                        newCycleDay = 1;
                        newIsSaverUsed = false;
                    }
                }
                else
                {
                    // Saver already used this month -> No rescue ticket left, reset streak
                    newCurrentStreak = 1;
                    newCycleDay = 1;
                    newIsSaverUsed = true;
                }
            }
        }
        else // gap >= 3
        {
            newCurrentStreak = 1;
            newCycleDay = 1;
            //  If it's a new month, reset saver usage to false; otherwise, keep the previous value
            newIsSaverUsed = !isNewMonth && streak.IsSaverUsed;
        }

        // Update streak entity
        streak.CurrentStreak = newCurrentStreak;
        streak.CycleDay = newCycleDay;
        streak.LastCheckIn = DateTime.SpecifyKind(now.DateTime, DateTimeKind.Utc);
        streak.IsSaverUsed = newIsSaverUsed;

        if (newCurrentStreak > streak.LongestStreak)
        {
            streak.LongestStreak = newCurrentStreak;
        }

        // Calculate reward based on cycle day (1-indexed)
        var rewardIndex = (newCycleDay - 1) % _streakSetting.DailyCheckInRewards.Count;
        var reward = _streakSetting.DailyCheckInRewards[rewardIndex];

        // Grant white coins
        await walletService.AddCoinsAsync(
            request.UserId,
            0,
            reward,
            OrderType.DailyCheckIn,
            cancellationToken
        );

        await context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
