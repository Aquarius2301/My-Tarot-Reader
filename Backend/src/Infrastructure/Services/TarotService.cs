using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyTarotReader.Application.Constants;
using MyTarotReader.Application.Contracts.Persistence;
using MyTarotReader.Application.Contracts.Services;
using MyTarotReader.Application.Dtos;
using MyTarotReader.Application.Exceptions;
using MyTarotReader.Domain.Entities;
using StackExchange.Redis;

namespace MyTarotReader.Infrastructure.Services;

public class TarotService : ITarotService
{
    private const string KeyPrefix = "tarot:draw:";
    private static readonly TimeSpan DrawCooldown = TimeSpan.FromHours(12);
    private readonly IConnectionMultiplexer _redis;
    private readonly IAppDbContext _context;

    public TarotService(IConnectionMultiplexer redis, IAppDbContext context)
    {
        _redis = redis;
        _context = context;
    }

    public async Task CreateDrawForAuthAsync(
        string cardCode,
        bool isReversed,
        Guid userId,
        CancellationToken cancellationToken = default
    )
    {
        var entity = new ReadHistory
        {
            CardCode = cardCode,
            IsReversed = isReversed,
            UserId = userId,
        };
        _context.ReadHistories.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<GetLastDrawnCardForAuthResponse?> GetLastDrawnCardForAuthAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    )
    {
        var entity = await _context
            .ReadHistories.Where(r => r.UserId == userId)
            .OrderByDescending(r => r.CreatedAt)
            .AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken);

        if (entity is null)
            return null;

        return new GetLastDrawnCardForAuthResponse(entity.CardCode, entity.IsReversed);
    }

    record DrawRecord(long DrawnAtUnixSeconds, string CardCode, bool IsReversed);

    public async Task<GetLastDrawnCardForGuestResponse> GetLastDrawnCardForGuestAsync(
        string guestKey,
        CancellationToken cancellationToken = default
    )
    {
        if (string.IsNullOrWhiteSpace(guestKey))
            throw new BadRequestException(ErrorMessageCode.Tarot.InvalidGuestKey);
        cancellationToken.ThrowIfCancellationRequested();

        var db = _redis.GetDatabase();
        var key = KeyPrefix + guestKey;
        var result = await db.StringGetWithExpiryAsync(key);
        if (!result.Value.HasValue)
        {
            return new GetLastDrawnCardForGuestResponse("", false, 0);
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
            record = null;
        }

        return new GetLastDrawnCardForGuestResponse(
            record?.CardCode ?? "",
            record?.IsReversed ?? false,
            remaining
        );
    }

    public async Task CreateDrawForGuestAsync(
        string guestKey,
        string cardCode,
        bool isReversed,
        CancellationToken cancellationToken = default
    )
    {
        if (string.IsNullOrWhiteSpace(guestKey))
            throw new BadRequestException(ErrorMessageCode.Tarot.InvalidGuestKey);
        if (!TarotConstants.IsValidCardCode(cardCode))
            throw new BadRequestException(ErrorMessageCode.Tarot.InvalidCard);
        cancellationToken.ThrowIfCancellationRequested();

        var db = _redis.GetDatabase();
        var key = KeyPrefix + guestKey;
        var record = new DrawRecord(
            DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            cardCode,
            isReversed
        );
        var value = JsonSerializer.Serialize(record);
        var set = await db.StringSetAsync(
            key,
            value,
            DrawCooldown,
            When.NotExists,
            CommandFlags.None
        );
        if (!set)
        {
            throw new TooManyRequestsException(ErrorMessageCode.Tarot.DrawnAlready);
        }
    }

    public async Task RemoveDrawForGuestAsync(
        string guestKey,
        CancellationToken cancellationToken = default
    )
    {
        if (string.IsNullOrWhiteSpace(guestKey))
            throw new BadRequestException(ErrorMessageCode.Tarot.InvalidGuestKey);
        cancellationToken.ThrowIfCancellationRequested();
        var db = _redis.GetDatabase();
        var key = KeyPrefix + guestKey;
        await db.KeyDeleteAsync(key);
    }
}
