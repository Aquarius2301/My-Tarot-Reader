using System.Text.Json;
using MediatR;
using MyTarotReader.Application.Constants.Tarot;
using StackExchange.Redis;

namespace MyTarotReader.Application.Features.Tarot;

public class GetLastDrawnCardForGuestQueryHandler(IConnectionMultiplexer redis)
    : IRequestHandler<GetLastDrawnCardForGuestQuery, GetLastDrawnCardForGuestResult?>
{
    public async Task<GetLastDrawnCardForGuestResult?> Handle(
        GetLastDrawnCardForGuestQuery request,
        CancellationToken _
    )
    {
        var db = redis.GetDatabase();
        var key = TarotConstant.KeyPrefix + request.GuestKey;
        var result = await db.StringGetWithExpiryAsync(key);
        if (!result.Value.HasValue)
        {
            return null;
        }

        long remaining =
            result.Expiry?.TotalSeconds > 0 ? (long)result.Expiry.Value.TotalSeconds : 0;

        DrawRecord? record;
        try
        {
            record = JsonSerializer.Deserialize<DrawRecord>(result.Value!);
        }
        catch (JsonException)
        {
            return null;
        }

        return record == null
            ? null
            : new GetLastDrawnCardForGuestResult(record.CardCode, record.IsReversed, remaining);
    }
}