using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MyTarotReader.Application.Common.Exceptions;
using MyTarotReader.Application.Common.Helpers;
using MyTarotReader.Application.Constants.Errors;
using MyTarotReader.Application.Persistence;
using MyTarotReader.Application.Services;
using MyTarotReader.Application.Settings;
using MyTarotReader.Domain.Enums;
using StreakEntity = MyTarotReader.Domain.Entities.Streak;

namespace MyTarotReader.Application.Features.Streak;

public class CheckInCommandHandler(
    IAppDbContext context,
    IWalletService walletService,
    IOptions<StreakSetting> streakSetting
) : IRequestHandler<CheckInCommand, Unit>
{
    private readonly StreakSetting _streakSetting = streakSetting.Value;

    public async Task<Unit> Handle(
        CheckInCommand request,
        CancellationToken cancellationToken = default
    )
    {
        var vietnamNow = StreakHelper.GetVietnamNow();

        await using var transaction = await context.Database.BeginTransactionAsync(
            cancellationToken
        );

        var streak = await context.Streaks.FirstOrDefaultAsync(
            s => s.UserId == request.UserId,
            cancellationToken
        );

        if (streak is null)
        {
            streak = new StreakEntity
            {
                UserId = request.UserId,
                CurrentStreak = 1,
                LongestStreak = 1,
                CycleDay = 1,
                LastCheckIn = vietnamNow,
            };
            context.Streaks.Add(streak);
        }
        else
        {
            if (StreakHelper.IsSameDay(streak.LastCheckIn, vietnamNow))
            {
                throw new BadRequestException(StreakErrorCode.AlreadyCheckedIn);
            }

            if (StreakHelper.IsConsecutiveDay(streak.LastCheckIn, vietnamNow))
            {
                streak.CurrentStreak++;
                streak.CycleDay = StreakHelper.GetNextCycleDay(
                    streak.CycleDay,
                    _streakSetting.CycleDays
                );
            }
            else
            {
                streak.CurrentStreak = 1;
                streak.CycleDay = 1;
                streak.IsSaverUsed = true;
            }

            StreakHelper.ResetSaverIfNewMonth(streak, vietnamNow);

            streak.LongestStreak = Math.Max(streak.LongestStreak, streak.CurrentStreak);
            streak.LastCheckIn = vietnamNow;
        }

        await context.SaveChangesAsync(cancellationToken);

        var rewards = _streakSetting.DailyCheckInRewards;
        var reward = rewards[(streak.CurrentStreak - 1) % rewards.Count];
        await walletService.AddCoinsAsync(
            request.UserId,
            0,
            reward,
            OrderType.DailyCheckIn,
            cancellationToken
        );

        await transaction.CommitAsync(cancellationToken);

        return Unit.Value;
    }
}
