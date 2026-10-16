using MediatR;
using Microsoft.EntityFrameworkCore;
using MyTarotReader.Application.Persistence;

namespace MyTarotReader.Application.Features.Shop;

public class GetPackagesQueryHandler(IAppDbContext context)
    : IRequestHandler<GetPackagesQuery, GetPackagesResult>
{
    public async Task<GetPackagesResult> Handle(
        GetPackagesQuery request,
        CancellationToken cancellationToken = default
    )
    {
        var packages = await context
            .ShopPackages.AsNoTracking()
            .Where(p => p.IsEnabled)
            .OrderBy(p => p.Price)
            .Select(p => new GetPackagesItem(
                p.Code,
                p.RedCoins,
                p.WhiteCoins,
                p.OldPrice,
                p.Price
            ))
            .ToListAsync(cancellationToken);

        return new GetPackagesResult(packages);
    }
}
