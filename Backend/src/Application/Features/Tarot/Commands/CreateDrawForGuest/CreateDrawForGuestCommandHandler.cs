using System.Text.Json;
using MediatR;
using MyTarotReader.Application.Common.Exceptions;
using MyTarotReader.Application.Constants.Errors;
using MyTarotReader.Application.Constants.Tarot;
using StackExchange.Redis;

namespace MyTarotReader.Application.Features.Tarot;

public class CreateDrawForGuestCommandHandler(IConnectionMultiplexer redis)
    : IRequestHandler<CreateDrawForGuestCommand>
{
    public async Task Handle(CreateDrawForGuestCommand request, CancellationToken _)
    {
        // ValidateGuestKey(guestKey);

        var db = redis.GetDatabase();
        var key = TarotConstant.KeyPrefix + request.GuestKey;
        var record = new DrawRecord(
            DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            request.CardCode,
            request.IsReversed
        );
        var value = JsonSerializer.Serialize(record);
        var set = await db.StringSetAsync(
            key,
            value,
            TarotConstant.DrawCooldown,
            When.NotExists,
            CommandFlags.None
        );
        if (!set)
        {
            throw new TooManyRequestsException(TarotReadingErrorCode.DrawnAlready);
        }

        await db.StringIncrementAsync(TarotConstant.GuestTotalKey);
    }
}