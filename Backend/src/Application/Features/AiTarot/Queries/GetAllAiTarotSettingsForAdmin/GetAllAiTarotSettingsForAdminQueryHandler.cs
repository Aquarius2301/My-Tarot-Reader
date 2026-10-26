using MediatR;
using Microsoft.EntityFrameworkCore;
using MyTarotReader.Application.Persistence;

namespace MyTarotReader.Application.Features.AiTarot;

public class GetAllAiTarotSettingsForAdminQueryHandler(IAppDbContext context)
    : IRequestHandler<GetAllAiTarotSettingsForAdminQuery, GetAllAiTarotSettingsForAdminResult>
{
    public async Task<GetAllAiTarotSettingsForAdminResult> Handle(
        GetAllAiTarotSettingsForAdminQuery request,
        CancellationToken cancellationToken = default
    )
    {
        var readingCounts = await context
            .AITarotReadingCounts.AsNoTracking()
            .OrderBy(s => s.CardCount)
            .Select(s => new AdminAiTarotReadingCountItem(
                s.Id,
                s.CardCount,
                s.WhiteCoins,
                s.PositionsEn,
                s.PositionsVi,
                s.MinWords,
                s.MaxWords,
                s.IsEnabled
            ))
            .ToListAsync(cancellationToken);

        var readingTypes = await context
            .AITarotReadingTypes.AsNoTracking()
            .OrderBy(s => s.Code)
            .Select(s => new AdminAiTarotReadingTypeItem(
                s.Id,
                s.Code,
                s.NameEn,
                s.NameVi,
                s.IsEnabled
            ))
            .ToListAsync(cancellationToken);

        return new GetAllAiTarotSettingsForAdminResult(readingCounts, readingTypes);
    }
}
