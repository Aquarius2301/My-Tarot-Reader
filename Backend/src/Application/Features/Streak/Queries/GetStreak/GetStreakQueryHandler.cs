using MediatR;
using Microsoft.EntityFrameworkCore;
using MyTarotReader.Application.Common.Time;
using MyTarotReader.Application.Persistence;
using StreakEntity = MyTarotReader.Domain.Entities.Streak;

namespace MyTarotReader.Application.Features.Streak;

public class GetStreakQueryHandler(IAppDbContext context, ITimeProvider timeProvider)
    : IRequestHandler<GetStreakQuery, GetStreakResult>
{
    public async Task<GetStreakResult> Handle(
        GetStreakQuery request,
        CancellationToken cancellationToken
    )
    {
        var now = timeProvider.GetVietnamNow();
        var today = now.DateTime.Date; // Convert to DateTime for comparison with entity's DateTime

        var streak = await context
            .Streaks.AsNoTracking()
            .FirstOrDefaultAsync(s => s.UserId == request.UserId, cancellationToken);

        // New user - return defaults without any streak logic
        if (streak == null)
        {
            return new GetStreakResult(
                CycleDay: 0,
                CurrentStreak: 0,
                LongestStreak: 0,
                IsSaverUsed: false,
                IsCheckedInToday: false
            );
        }

        var lastCheckInDate =
            streak.LastCheckIn == DateTime.MinValue ? today.AddDays(-100) : streak.LastCheckIn.Date;

        var gap = (today - lastCheckInDate).Days;

        bool isSaverUsedForDisplay;

        bool isNewMonth =
            lastCheckInDate.Month != today.Month || lastCheckInDate.Year != today.Year;

        if (streak.LastCheckIn == DateTime.MinValue || isNewMonth)
        {
            isSaverUsedForDisplay = false;
        }
        else
        {
            isSaverUsedForDisplay = streak.IsSaverUsed;
        }

        int currentStreak = streak.CurrentStreak;
        int cycleDay = streak.CycleDay;
        bool isCheckedInToday = false;

        switch (gap)
        {
            case 0:
                isCheckedInToday = true;
                break;

            case 1:
                isCheckedInToday = false;
                break;

            case 2:
                if (streak.LastCheckIn == DateTime.MinValue || isNewMonth)
                {
                    currentStreak = 0;
                    cycleDay = 0;
                    isSaverUsedForDisplay = false;
                }
                else
                {
                    if (!isSaverUsedForDisplay)
                    {
                        if (currentStreak > 1) // Only use saver when streak is greater than 1 (day 2)
                        {
                            isSaverUsedForDisplay = true;
                        }
                        else
                        {
                            currentStreak = 0;
                            cycleDay = 0;
                            isSaverUsedForDisplay = false;
                        }
                    }
                    else
                    {
                        currentStreak = 0;
                        cycleDay = 0;
                        isSaverUsedForDisplay = true;
                    }
                }
                break;

            default:
                currentStreak = 0;
                cycleDay = 0;
                isSaverUsedForDisplay = !isNewMonth && streak.IsSaverUsed; //If new month, reset saver usage to false, otherwise keep the previous value

                break;
        }

        return new GetStreakResult(
            CycleDay: cycleDay,
            CurrentStreak: currentStreak,
            LongestStreak: streak.LongestStreak,
            IsSaverUsed: isSaverUsedForDisplay,
            IsCheckedInToday: isCheckedInToday
        );
    }
}
