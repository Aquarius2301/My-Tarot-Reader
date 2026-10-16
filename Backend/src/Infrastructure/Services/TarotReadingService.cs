using System.Text.Json;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using MyTarotReader.Application.Common.Exceptions;
using MyTarotReader.Application.Common.Validators;
using MyTarotReader.Application.Constants.Errors;
using MyTarotReader.Application.Contracts.Persistence;
using MyTarotReader.Application.Contracts.Services;
using MyTarotReader.Domain.Entities;
using StackExchange.Redis;

namespace MyTarotReader.Infrastructure.Services;

public class TarotReadingService(
    IAppDbContext context,
    IConnectionMultiplexer redis,
    IValidator<CreateDrawForAuthRequest> createDrawForAuthValidator,
    IValidator<CreateDrawForGuestRequest> createDrawForGuestValidator
) : ITarotReadingService
{
    private readonly IAppDbContext _context = context;
    private const string KeyPrefix = "tarot:draw:";
    private static readonly TimeSpan DrawCooldown = TimeSpan.FromHours(12);
    private readonly IConnectionMultiplexer _redis = redis;
    private readonly IValidator<CreateDrawForAuthRequest> _createDrawForAuthValidator =
        createDrawForAuthValidator;
    private readonly IValidator<CreateDrawForGuestRequest> _createDrawForGuestValidator =
        createDrawForGuestValidator;

    public async Task CreateDrawForAuthAsync(
        CreateDrawForAuthRequest request,
        Guid userId,
        CancellationToken cancellationToken = default
    )
    {
        ValidationHelper.ValidateOrThrow(_createDrawForAuthValidator, request);

        var entity = new TarotReading
        {
            CardCode = request.CardCode,
            IsReversed = request.IsReversed,
            UserId = userId,
        };

        _context.TarotReadings.Add(entity);

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<GetLastDrawnCardForAuthResult?> GetLastDrawnCardForAuthAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    )
    {
        var tarotCard = await _context
            .TarotReadings.AsNoTracking()
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.CreatedAt)
            .Select(t => new GetLastDrawnCardForAuthResult(t.CardCode, t.IsReversed))
            .FirstOrDefaultAsync(cancellationToken);

        return tarotCard;
    }

    public async Task CreateDrawForGuestAsync(
        CreateDrawForGuestRequest request,
        string guestKey,
        CancellationToken cancellationToken = default
    )
    {
        ValidateGuestKey(guestKey);

        ValidationHelper.ValidateOrThrow(_createDrawForGuestValidator, request);

        var db = _redis.GetDatabase();
        var key = KeyPrefix + guestKey;
        var record = new DrawRecord(
            DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            request.CardCode,
            request.IsReversed
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
            throw new TooManyRequestsException(TarotReadingErrorCode.DrawnAlready);
        }
    }

    record DrawRecord(long DrawnAtUnixSeconds, string CardCode, bool IsReversed);

    public async Task<GetLastDrawnCardForGuestResult?> GetLastDrawnCardForGuestAsync(
        string guestKey,
        CancellationToken cancellationToken = default
    )
    {
        ValidateGuestKey(guestKey);

        var db = _redis.GetDatabase();
        var key = KeyPrefix + guestKey;
        var result = await db.StringGetWithExpiryAsync(key);
        if (!result.Value.HasValue)
        {
            return null;
        }

        long remaining =
            result.Expiry?.TotalSeconds > 0 ? (long)result.Expiry.Value.TotalSeconds : 0;

        DrawRecord? record = null;
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

    public async Task RemoveDrawForGuestAsync(
        string guestKey,
        CancellationToken cancellationToken = default
    )
    {
        ValidateGuestKey(guestKey);

        var db = _redis.GetDatabase();
        var key = KeyPrefix + guestKey;
        await db.KeyDeleteAsync(key);
    }

    public async Task<GetAllReadingResult> GetAllReadingAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    )
    {
        var history = await _context
            .TarotReadings.AsNoTracking()
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new GetAllReadingItem(r.Id, r.CardCode, r.IsReversed, r.CreatedAt))
            .ToListAsync(cancellationToken);

        return new GetAllReadingResult(history);
    }

    public async Task DeleteReadingAsync(
        Guid userId,
        Guid readingId,
        CancellationToken cancellationToken = default
    )
    {
        var record =
            await _context.TarotReadings.FirstOrDefaultAsync(
                r => r.Id == readingId && r.UserId == userId,
                cancellationToken
            ) ?? throw new NotFoundException(TarotReadingErrorCode.NotFound);

        record.DeletedAt = DateTimeOffset.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Validates that the guest key is present, since a missing "X-Device-Id" header
    /// would otherwise share a single Redis draw slot across all header-less clients.
    /// </summary>
    /// <param name="guestKey">The guest device identifier from the "X-Device-Id" header.</param>
    /// <exception cref="BadRequestException">Thrown when the guest key is empty or whitespace.</exception>
    private static void ValidateGuestKey(string guestKey)
    {
        if (string.IsNullOrWhiteSpace(guestKey))
        {
            throw new BadRequestException(TarotReadingErrorCode.InvalidGuestKey);
        }
    }
}
