using System;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MyTarotReader.Application.Persistence;

namespace MyTarotReader.Application.Features.AiTarot;

public class GetAiTarotSettingQueryHandler(IAppDbContext context)
    : IRequestHandler<GetAiTarotSettingQuery, GetAiTarotSettingResult>
{
    public async Task<GetAiTarotSettingResult> Handle(
        GetAiTarotSettingQuery request,
        CancellationToken cancellationToken
    )
    {
        var count = await context
            .AITarotReadingCounts.Where(s => s.IsEnabled)
            .AsNoTracking()
            .Select(s => new AiTarotReadingCountItem(
                s.CardCount,
                s.WhiteCoins,
                s.PositionsEn,
                s.PositionsVi
            ))
            .ToListAsync(cancellationToken);

        var types = await context
            .AITarotReadingTypes.Where(s => s.IsEnabled)
            .AsNoTracking()
            .Select(s => new AiTarotReadingTypeItem(s.Code, s.NameEn, s.NameVi))
            .ToListAsync(cancellationToken);

        return new GetAiTarotSettingResult(count, types);
    }
}
