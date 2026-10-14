using MediatR;
using Microsoft.EntityFrameworkCore;
using MyTarotReader.Application.Common.Helpers;
using MyTarotReader.Application.Persistence;

namespace MyTarotReader.Application.Features.AiDeepTarot;

public class GetAllAiDeepTarotReadingsQueryHandler(IAppDbContext context)
    : IRequestHandler<GetAllAiDeepTarotReadingsQuery, GetAllAiDeepTarotReadingsResult>
{
    public async Task<GetAllAiDeepTarotReadingsResult> Handle(
        GetAllAiDeepTarotReadingsQuery request,
        CancellationToken cancellationToken = default
    )
    {
        var (page, pageSize) = PaginationHelper.Normalize(request.Page, request.PageSize);

        var query = context
            .AIDeepTarotReadings.AsNoTracking()
            .Where(r => r.UserId == request.UserId)
            .OrderByDescending(r => r.CreatedAt)
            .ThenByDescending(r => r.Id);

        var total = await query.CountAsync(cancellationToken);

        var readings = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(r => new
            {
                r.Id,
                r.Topic,
                r.Title,
                r.AnswerSummary,
                r.Cards,
                r.Question,
                r.CreatedAt,
            })
            .ToListAsync(cancellationToken);

        var items = readings
            .Select(r => new GetAllAiDeepTarotReadingItem(
                r.Id,
                r.Topic,
                r.Title,
                r.AnswerSummary,
                AiDeepTarotHelper.DeserializeCards(r.Cards),
                r.CreatedAt,
                r.Question
            ))
            .ToList();

        return new GetAllAiDeepTarotReadingsResult(items, page, pageSize, total);
    }
}
