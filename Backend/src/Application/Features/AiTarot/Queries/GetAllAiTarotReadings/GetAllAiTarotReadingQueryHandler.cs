using MediatR;
using Microsoft.EntityFrameworkCore;
using MyTarotReader.Application.Common.Helpers;
using MyTarotReader.Application.Features.AiTarot;
using MyTarotReader.Application.Persistence;

namespace MyTarotReader.Application.Features.AITarot;

public class GetAllAiTarotReadingQueryHandler(IAppDbContext context)
    : IRequestHandler<GetAllAiTarotReadingQuery, GetAllAiTarotReadingResult>
{
    public async Task<GetAllAiTarotReadingResult> Handle(
        GetAllAiTarotReadingQuery request,
        CancellationToken cancellationToken = default
    )
    {
        var (page, pageSize) = PaginationHelper.Normalize(request.Page, request.PageSize);

        var query = context
            .AITarotReadings.AsNoTracking()
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
                r.CardCount,
                r.QuestionType,
                r.Title,
                r.AnswerSummary,
                r.Cards,
                r.CreatedAt,
            })
            .ToListAsync(cancellationToken);

        var items = readings
            .Select(r => new GetAllAiTarotReadingItem(
                r.Id,
                r.CardCount,
                r.QuestionType,
                r.Title,
                r.AnswerSummary,
                AiTarotHelper.DeserializeCards(r.Cards),
                r.CreatedAt
            ))
            .ToList();

        return new GetAllAiTarotReadingResult(items, page, pageSize, total);
    }
}
