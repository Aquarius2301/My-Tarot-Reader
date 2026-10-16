using MediatR;

namespace MyTarotReader.Application.Features.Shop;

public record GetPackagesQuery : IRequest<GetPackagesResult>;

/// <summary>
/// A purchasable coin package exposed by the shop.
/// </summary>
/// <param name="Code">The unique package code used when creating an order.</param>
/// <param name="RedCoins">The number of red coins granted on payment.</param>
/// <param name="WhiteCoins">The number of bonus white coins granted on payment.</param>
/// <param name="OldPrice">The original price in VND, shown struck through when discounted.</param>
/// <param name="Price">The current package price in VND.</param>
public record GetPackagesItem(string Code, int RedCoins, int WhiteCoins, int OldPrice, int Price);

/// <summary>Result of listing the purchasable packages.</summary>
/// <param name="Packages">The available packages.</param>
public record GetPackagesResult(List<GetPackagesItem> Packages);
