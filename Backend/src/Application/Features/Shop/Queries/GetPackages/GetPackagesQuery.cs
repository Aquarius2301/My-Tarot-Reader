using MediatR;

namespace MyTarotReader.Application.Features.Shop;

public record GetPackagesQuery : IRequest<GetPackagesResult>;

/// <summary>
/// A purchasable red-coin package exposed by the shop.
/// </summary>
/// <param name="Code">The unique package code used when creating an order.</param>
/// <param name="RedCoins">The number of red coins granted on payment.</param>
/// <param name="PriceVnd">The package price in VND.</param>
public record GetPackagesItem(string Code, int RedCoins, int PriceVnd);

/// <summary>Result of listing the purchasable packages.</summary>
/// <param name="Packages">The available packages.</param>
public record GetPackagesResult(List<GetPackagesItem> Packages);
