using MediatR;
using Microsoft.EntityFrameworkCore;
using MyTarotReader.Application.Persistence;

namespace MyTarotReader.Application.Features.Tarot;

public class GetLastDrawnCardForAuthQueryHandler(IAppDbContext context)
    : IRequestHandler<GetLastDrawnCardForAuthQuery, GetLastDrawnCardForAuthResult?>
{
    public async Task<GetLastDrawnCardForAuthResult?> Handle(
        GetLastDrawnCardForAuthQuery request,
        CancellationToken cancellationToken
    )
    {
        var tarotCard = await context
            .TarotReadings.AsNoTracking()
            .Where(r => r.UserId == request.UserId)
            .OrderByDescending(r => r.CreatedAt)
            .Select(t => new GetLastDrawnCardForAuthResult(t.CardCode, t.IsReversed))
            .FirstOrDefaultAsync(cancellationToken);

        return tarotCard;
    }
}