using MediatR;
using Microsoft.EntityFrameworkCore;
using MyTarotReader.Application.Common.Helpers;
using MyTarotReader.Application.Persistence;

namespace MyTarotReader.Application.Features.Tarot;

public class GetAllReadingQueryHandler(IAppDbContext context)
    : IRequestHandler<GetAllReadingQuery, GetAllReadingResult>
{
    public async Task<GetAllReadingResult> Handle(
        GetAllReadingQuery request,
        CancellationToken cancellationToken
    )
    {
        var (page, pageSize) = PaginationHelper.Normalize(request.Page, request.PageSize);

        var query = context
            .TarotReadings.AsNoTracking()
            .Where(r => r.UserId == request.UserId)
            .OrderByDescending(r => r.CreatedAt)
            .ThenByDescending(r => r.Id);

        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(r => new GetAllReadingItem(r.Id, r.CardCode, r.IsReversed, r.CreatedAt))
            .ToListAsync(cancellationToken);

        return new GetAllReadingResult(items, page, pageSize, total);
    }
}