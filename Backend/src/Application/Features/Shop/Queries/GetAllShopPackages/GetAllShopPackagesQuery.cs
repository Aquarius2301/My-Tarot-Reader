using MediatR;

namespace MyTarotReader.Application.Features.Shop;

public record GetAllShopPackagesQuery : IRequest<GetAllShopPackagesResult>;

/// <summary> A shop package as shown to admins, including disabled packages. </summary>
public record GetAllShopPackagesItem(
    Guid Id,
    string Code,
    int RedCoins,
    int WhiteCoins,
    int OldPrice,
    int Price,
    bool IsEnabled
);

/// <summary>Result of listing every shop package for admins.</summary>
public record GetAllShopPackagesResult(List<GetAllShopPackagesItem> Items);
