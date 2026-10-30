using MediatR;

namespace MyTarotReader.Application.Features.Shop;

public record DeleteShopPackageCommand(Guid Id) : IRequest<Unit>;
