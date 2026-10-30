using MediatR;

namespace MyTarotReader.Application.Features.Shop;

public record UpdateShopPackageCommand(
    Guid Id,
    string Code,
    int RedCoins,
    int WhiteCoins,
    int OldPrice,
    int Price,
    bool IsEnabled
) : IRequest<Unit>;
