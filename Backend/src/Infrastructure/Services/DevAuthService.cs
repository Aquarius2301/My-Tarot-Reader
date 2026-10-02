using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MyTarotReader.Application.Contracts.Common;
using MyTarotReader.Application.Contracts.Persistence;
using MyTarotReader.Application.Contracts.Services;
using MyTarotReader.Application.Settings;
using MyTarotReader.Domain.Entities;
using MyTarotReader.Domain.Enums;

namespace MyTarotReader.Infrastructure.Services;

/// <summary>
/// Mints real JWTs for a single seeded development user, so JWT-protected endpoints
/// can be exercised from Swagger UI without going through Google OAuth.
/// </summary>
/// <remarks>
/// No welcome email is queued, because the seeded user is not a real registration.
/// </remarks>
public class DevAuthService(
    IOptions<JwtSetting> jwtSetting,
    IOptions<DevAuthSetting> devAuthSetting,
    IAppDbContext context,
    IJwtTokenGenerator jwtTokenGenerator
) : IDevAuthService
{
    private readonly JwtSetting _jwtSetting = jwtSetting.Value;
    private readonly DevAuthSetting _devAuthSetting = devAuthSetting.Value;
    private readonly IAppDbContext _context = context;
    private readonly IJwtTokenGenerator _jwtTokenGenerator = jwtTokenGenerator;

    public async Task<CreateDevTokenResult> CreateDevTokenAsync(
        CreateDevTokenRequest request,
        string deviceFingerprint,
        CancellationToken cancellationToken = default
    )
    {
        var fingerprint = string.IsNullOrWhiteSpace(deviceFingerprint)
            ? _devAuthSetting.DefaultDeviceFingerprint
            : deviceFingerprint;

        var user =
            await _context.Users.FirstOrDefaultAsync(
                u => u.ProviderKey == _devAuthSetting.ProviderKey,
                cancellationToken
            ) ?? SeedDevUser(request.Role);

        if (request.Role.HasValue && user.Role != request.Role.Value)
        {
            user.Role = request.Role.Value;
        }

        var accessToken = _jwtTokenGenerator.GenerateAccessToken(user);
        var refreshToken = _jwtTokenGenerator.GenerateRefreshToken();

        await RotateRefreshTokenAsync(user.Id, fingerprint, refreshToken, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);

        return new CreateDevTokenResult(
            accessToken,
            refreshToken,
            _jwtSetting.AccessTokenDurationMinutes,
            _jwtSetting.RefreshTokenDurationDays,
            user.Id,
            user.Email,
            user.FullName,
            await SumActiveWhiteCoinsAsync(user.Id, cancellationToken),
            await GetRedCoinAsync(user.Id, cancellationToken),
            user.Role
        );
    }

    #region Private Methods

    /// <summary>
    /// Creates the development user together with the wallet, white coin batch,
    /// first-login order and streak a real registration would produce.
    /// </summary>
    /// <param name="role">The role requested for the new user, defaulting to <see cref="UserRole.Registered"/>.</param>
    private User SeedDevUser(UserRole? role)
    {
        var user = new User
        {
            FullName = _devAuthSetting.FullName,
            Email = _devAuthSetting.Email,
            Picture = string.Empty,
            ProviderKey = _devAuthSetting.ProviderKey,
            Role = role ?? UserRole.Registered,
        };

        _context.Users.Add(user);

        var whiteCoinBatch = new WhiteCoinBatch
        {
            Amount = _devAuthSetting.InitialWhiteCoins,
            RemainingAmount = _devAuthSetting.InitialWhiteCoins,
            ExpiredAt = DateTimeOffset.UtcNow.AddDays(_devAuthSetting.WhiteCoinExpireDays),
        };

        _context.Wallets.Add(
            new Wallet
            {
                UserId = user.Id,
                WhiteCoinBatches = [whiteCoinBatch],
            }
        );

        _context.Orders.Add(
            new Order
            {
                UserId = user.Id,
                Amount = _devAuthSetting.InitialWhiteCoins,
                Description = "Development user seed",
                Type = OrderType.FirstLogin,
                OrderDetails =
                [
                    new OrderDetail
                    {
                        WhiteCoinBatchId = whiteCoinBatch.Id,
                        Amount = _devAuthSetting.InitialWhiteCoins,
                    },
                ],
            }
        );

        _context.Streaks.Add(new Streak { UserId = user.Id });

        return user;
    }

    /// <summary>
    /// Soft-deletes any refresh token already bound to the same user and device,
    /// then issues a new one, mirroring the rotation done on a real login.
    /// </summary>
    private async Task RotateRefreshTokenAsync(
        Guid userId,
        string deviceFingerprint,
        string newToken,
        CancellationToken cancellationToken
    )
    {
        var existingToken = await _context.RefreshTokens.FirstOrDefaultAsync(
            rt => rt.UserId == userId && rt.DeviceFingerprint == deviceFingerprint,
            cancellationToken
        );

        if (existingToken != null)
        {
            existingToken.DeletedAt = DateTimeOffset.UtcNow;
        }

        _context.RefreshTokens.Add(
            new RefreshToken
            {
                UserId = userId,
                Token = newToken,
                DeviceFingerprint = deviceFingerprint,
                ExpiresAt = DateTimeOffset.UtcNow.AddDays(_jwtSetting.RefreshTokenDurationDays),
            }
        );
    }

    /// <summary>
    /// Sums the remaining amount of the user's unexpired, non-depleted white coin batches.
    /// </summary>
    private async Task<int> SumActiveWhiteCoinsAsync(Guid userId, CancellationToken cancellationToken) =>
        await _context
            .WhiteCoinBatches.AsNoTracking()
            .Where(b =>
                b.Wallet.UserId == userId
                && b.RemainingAmount > 0
                && b.ExpiredAt >= DateTimeOffset.UtcNow
            )
            .SumAsync(b => b.RemainingAmount, cancellationToken);

    private async Task<int> GetRedCoinAsync(Guid userId, CancellationToken cancellationToken) =>
        await _context
            .Wallets.AsNoTracking()
            .Where(w => w.UserId == userId)
            .Select(w => w.RedCoin)
            .FirstOrDefaultAsync(cancellationToken);

    #endregion
}
