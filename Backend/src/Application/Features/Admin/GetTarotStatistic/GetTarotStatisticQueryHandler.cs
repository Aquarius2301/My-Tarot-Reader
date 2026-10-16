using MediatR;
using Microsoft.EntityFrameworkCore;
using MyTarotReader.Application.Constants.Tarot;
using MyTarotReader.Application.Persistence;
using StackExchange.Redis;

namespace MyTarotReader.Application.Features.Admin;

public class GetTarotStatisticQueryHandler(IAppDbContext context, IConnectionMultiplexer redis)
    : IRequestHandler<GetTarotStatisticQuery, GetTarotStatisticResult>
{
    public async Task<GetTarotStatisticResult> Handle(
        GetTarotStatisticQuery request,
        CancellationToken cancellationToken = default
    )
    {
        var db = redis.GetDatabase();
        var guestTotal = await db.StringGetAsync(TarotConstant.GuestTotalKey);
        var totalGuest = guestTotal.HasValue && long.TryParse(guestTotal, out var guestCount)
            ? (int)Math.Min(guestCount, int.MaxValue)
            : 0;

        var totalRegistered = await context.TarotReadings.CountAsync(cancellationToken);

        var aiGroups = await context
            .AITarotReadings.AsNoTracking()
            .GroupBy(r => new { r.QuestionType, r.CardCount })
            .Select(g => new
            {
                g.Key.QuestionType,
                g.Key.CardCount,
                Count = g.Count(),
            })
            .ToListAsync(cancellationToken);

        var aiBreakdown = aiGroups
            .Select(g => new GetTarotStatisticAiTarotItem(g.QuestionType, g.CardCount, g.Count))
            .OrderByDescending(x => x.Count)
            .ToList();

        return new GetTarotStatisticResult(
            totalGuest,
            totalRegistered,
            aiBreakdown.Sum(x => x.Count),
            aiBreakdown
        );
    }
}
