using MediatR;

namespace MyTarotReader.Application.Features.Wallet;

public record ConvertRedToWhiteCommand(Guid UserId, int RedCoins) : IRequest<Unit>;
