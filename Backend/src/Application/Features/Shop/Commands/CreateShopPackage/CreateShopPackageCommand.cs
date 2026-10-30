using MediatR;

namespace MyTarotReader.Application.Features.Shop;

public record CreateShopPackageCommand(
    string Code,
    int RedCoins,
    int WhiteCoins,
    int OldPrice,
    int Price,
    bool IsEnabled
) : IRequest<Guid>;
