using MediatR;
using Microsoft.EntityFrameworkCore;
using MyTarotReader.Application.Persistence;

namespace MyTarotReader.Application.Features.Shop;

public class GetAllShopPackagesQueryHandler(IAppDbContext context)
    : IRequestHandler<GetAllShopPackagesQuery, GetAllShopPackagesResult>
{
    public async Task<GetAllShopPackagesResult> Handle(
        GetAllShopPackagesQuery request,
        CancellationToken cancellationToken = default
    )
    {
        var items = await context
            .ShopPackages.AsNoTracking()
            .OrderBy(p => p.Price)
            .Select(p => new GetAllShopPackagesItem(
                p.Id,
                p.Code,
                p.RedCoins,
                p.WhiteCoins,
                p.OldPrice,
                p.Price,
                p.IsEnabled
            ))
            .ToListAsync(cancellationToken);

        return new GetAllShopPackagesResult(items);
    }
}
