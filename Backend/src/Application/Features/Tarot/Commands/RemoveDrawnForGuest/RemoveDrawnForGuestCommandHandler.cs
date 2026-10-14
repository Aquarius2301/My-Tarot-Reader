using MediatR;
using MyTarotReader.Application.Constants.Tarot;
using StackExchange.Redis;

namespace MyTarotReader.Application.Features.Tarot;

public class RemoveDrawnForGuestCommandHandler(IConnectionMultiplexer redis)
    : IRequestHandler<RemoveDrawnForGuestCommand>
{
    public async Task Handle(
        RemoveDrawnForGuestCommand request,
        CancellationToken cancellationToken
    )
    {
        var db = redis.GetDatabase();
        var guestKey = request.GuestId;
        var key = TarotConstant.KeyPrefix + guestKey;
        await db.KeyDeleteAsync(key);
    }
}