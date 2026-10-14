using MediatR;
using Microsoft.Extensions.Options;
using MyTarotReader.Application.Settings;

namespace MyTarotReader.Application.Features.Shop;

public class GetPackagesQueryHandler(IOptions<ShopSetting> shopSetting)
    : IRequestHandler<GetPackagesQuery, GetPackagesResult>
{
    private readonly ShopSetting _shopSetting = shopSetting.Value;

    public Task<GetPackagesResult> Handle(
        GetPackagesQuery request,
        CancellationToken cancellationToken = default
    )
    {
        var packages = _shopSetting
            .Packages.Select(p => new GetPackagesItem(p.Code, p.RedCoins, p.PriceVnd))
            .ToList();

        return Task.FromResult(new GetPackagesResult(packages));
    }
}
