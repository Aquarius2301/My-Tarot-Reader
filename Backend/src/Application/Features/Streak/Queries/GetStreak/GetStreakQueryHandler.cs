using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MyTarotReader.Application.Common.Helpers;
using MyTarotReader.Application.Persistence;
using MyTarotReader.Application.Settings;

namespace MyTarotReader.Application.Features.Streak;

public class GetStreakQueryHandler(IAppDbContext context, IOptions<StreakSetting> streakSetting)
    : IRequestHandler<GetStreakQuery, GetStreakResult>
{
    private readonly StreakSetting _streakSetting = streakSetting.Value;

    public async Task<GetStreakResult> Handle(
        GetStreakQuery request,
        CancellationToken cancellationToken = default
    )
    {
        var streak = await context.Streaks.FirstOrDefaultAsync(
            s => s.UserId == request.UserId,
            cancellationToken
        );

        if (streak is null)
        {
            return new GetStreakResult(0, 0, 0, false, false);
        }

        var vietnamNow = StreakHelper.GetVietnamNow();

        var resetStreak = StreakHelper.IsBroken(streak.LastCheckIn, vietnamNow);

        if (resetStreak)
        {
            streak.CurrentStreak = 0;
            streak.CycleDay = 0;
            streak.IsSaverUsed = true;
        }

        var resetSaver = StreakHelper.ResetSaverIfNewMonth(streak, vietnamNow);

        if (resetSaver || resetStreak)
        {
            await context.SaveChangesAsync(cancellationToken);
        }

        return new GetStreakResult(
            StreakHelper.GetCycleDayToShow(streak.CycleDay, _streakSetting.CycleDays),
            streak.CurrentStreak,
            streak.LongestStreak,
            streak.IsSaverUsed,
            StreakHelper.IsSameDay(streak.LastCheckIn, vietnamNow)
        );
    }
}
