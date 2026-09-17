using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MyTarotReader.Application.Common.Exceptions;
using MyTarotReader.Application.Constants.Errors;
using MyTarotReader.Application.Contracts.Common;
using MyTarotReader.Application.Contracts.Persistence;
using MyTarotReader.Application.Contracts.Services;
using MyTarotReader.Application.Settings;
using MyTarotReader.Domain.Entities;
using MyTarotReader.Domain.Enums;

namespace MyTarotReader.Infrastructure.Services;

public class AuthService(
    IOptions<JwtSetting> jwtSetting,
    IOptions<WalletSetting> walletSetting,
    IAppDbContext context,
    IEmailHandler emailHandler,
    IGoogleAuthValidator googleAuthValidator,
    IJwtTokenGenerator jwtTokenGenerator
) : IAuthService
{
    private readonly WalletSetting _walletSetting = walletSetting.Value;
    private readonly IAppDbContext _context = context;

    private readonly IEmailHandler _emailHandler = emailHandler;
    private readonly IGoogleAuthValidator _googleAuthValidator = googleAuthValidator;
    private readonly IJwtTokenGenerator _jwtTokenGenerator = jwtTokenGenerator;
    private readonly JwtSetting _jwtSetting = jwtSetting.Value;

    public async Task<GoogleLoginResult> GoogleLoginAsync(
        GoogleLoginRequest request,
        string deviceFingerprint,
        CancellationToken cancellationToken = default
    )
    {
        var payload =
            await _googleAuthValidator.ValidateAsync(request.Credential, cancellationToken)
            ?? throw new BadRequestException(AuthErrorCode.InvalidKeyCredential);

        // Check if the user already exists in the database based on the provider key (Google ID)
        var user = await _context.Users.FirstOrDefaultAsync(
            u => u.ProviderKey == payload.ProviderKey,
            cancellationToken
        );

        if (user == null)
        {
            user = new User
            {
                FullName = payload.Name,
                Email = payload.Email,
                Picture = payload.Picture,
                ProviderKey = payload.ProviderKey,
                Role = UserRole.Registered,
            };

            _context.Users.Add(user);

            var wallet = new Wallet
            {
                UserId = user.Id,
                WhiteCoinBatches =
                [
                    new()
                    {
                        Amount = _walletSetting.InitialWhiteCoins,
                        RemainingAmount = _walletSetting.InitialWhiteCoins,
                        ExpiredAt = DateTimeOffset.UtcNow.AddDays(_walletSetting.ExpireDays),
                    },
                ],
            };
            _context.Wallets.Add(wallet);

            _ = _emailHandler.SendWelcomeEmailAsync(
                user.Email,
                user.FullName,
                request.Locale,
                cancellationToken
            );
        }
        else
        {
            // Sync user information with the latest data from Google
            user.FullName = payload.Name;
            user.Picture = payload.Picture;
            user.ProviderKey = payload.ProviderKey;
        }

        var accessToken = _jwtTokenGenerator.GenerateAccessToken(user);
        var refreshToken = _jwtTokenGenerator.GenerateRefreshToken();

        // Check if a refresh token already exists for this user and device fingerprint
        var existingToken = await _context.RefreshTokens.FirstOrDefaultAsync(
            rt => rt.UserId == user.Id && rt.DeviceFingerprint == deviceFingerprint,
            cancellationToken
        );

        if (existingToken != null)
        {
            existingToken.DeletedAt = DateTimeOffset.UtcNow;
        }

        AddRefreshToken(user, deviceFingerprint, refreshToken);

        await _context.SaveChangesAsync(cancellationToken);

        return new GoogleLoginResult(
            accessToken,
            refreshToken,
            _jwtSetting.AccessTokenDurationMinutes,
            _jwtSetting.RefreshTokenDurationDays
        );
    }

    public async Task<GoogleLoginResult> RefreshAsync(
        string refreshToken,
        string deviceFingerprint,
        CancellationToken cancellationToken = default
    )
    {
        // get the token from the database, including the deleted token.
        var existedToken =
            await _context
                .RefreshTokens.Include(x => x.User)
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(r => r.Token == refreshToken, cancellationToken)
            ?? throw new UnauthorizedException(AuthErrorCode.InvalidRefreshToken);

        // Revoke all active tokens when the token is deleted, expired, or the device fingerprint does not match since it may be reused

        // If the token is expired, it may be reused
        if (existedToken.DeletedAt != null)
        {
            await RevokeAllActiveTokensAsync(existedToken.UserId, cancellationToken);
            throw new UnauthorizedException(AuthErrorCode.InvalidRefreshToken);
        }

        // If the user has been deleted
        if (existedToken.User.DeletedAt != null)
        {
            await RevokeAllActiveTokensAsync(existedToken.UserId, cancellationToken);
            throw new UnauthorizedException(AuthErrorCode.InvalidRefreshToken);
        }

        // If the device fingerprint does not match, it may be a token theft attempt
        if (existedToken.DeviceFingerprint != deviceFingerprint)
        {
            await RevokeAllActiveTokensAsync(existedToken.UserId, cancellationToken);
            throw new UnauthorizedException(AuthErrorCode.InvalidRefreshToken);
        }

        var now = DateTimeOffset.UtcNow;

        // If the token is expired, just revoke it and ask the user to log in again.
        if (existedToken.ExpiresAt < now)
        {
            existedToken.DeletedAt = now;
            await _context.SaveChangesAsync(cancellationToken);

            throw new UnauthorizedException(AuthErrorCode.InvalidRefreshToken);
        }

        // If the token is valid, generate a new access token and refresh token
        var newAccessToken = _jwtTokenGenerator.GenerateAccessToken(existedToken.User);
        var newRefreshToken = _jwtTokenGenerator.GenerateRefreshToken();

        existedToken.DeletedAt = now;

        AddRefreshToken(existedToken.User, deviceFingerprint, newRefreshToken);

        await _context.SaveChangesAsync(cancellationToken);

        return new GoogleLoginResult(
            newAccessToken,
            newRefreshToken,
            _jwtSetting.AccessTokenDurationMinutes,
            _jwtSetting.RefreshTokenDurationDays
        );
    }

    public async Task LogoutAsync(
        string refreshToken,
        CancellationToken cancellationToken = default
    )
    {
        var stored = await _context.RefreshTokens.FirstOrDefaultAsync(
            r => r.Token == refreshToken,
            cancellationToken
        );
        if (stored != null)
        {
            stored.DeletedAt = DateTimeOffset.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<GetCurrentUserResult> GetCurrentUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    )
    {
        var user =
            await _context
                .Users.AsNoTracking()
                .Where(u => u.Id == userId)
                .Select(x => new GetCurrentUserResult(
                    x.Id,
                    x.FullName,
                    x.Email,
                    x.Picture,
                    x.Wallet.WhiteCoinBatches.Where(b =>
                            b.RemainingAmount > 0 && b.ExpiredAt >= DateTimeOffset.UtcNow
                        )
                        .Sum(b => b.RemainingAmount),
                    x.Wallet.RedCoin,
                    x.Role
                ))
                .FirstOrDefaultAsync(cancellationToken)
            ?? throw new UnauthorizedException();

        return user;
    }

    #region Private Methods

    private void AddRefreshToken(User user, string deviceFingerprint, string token)
    {
        var refreshToken = new RefreshToken
        {
            UserId = user.Id,
            Token = token,
            DeviceFingerprint = deviceFingerprint,
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(_jwtSetting.RefreshTokenDurationDays),
        };

        _context.RefreshTokens.Add(refreshToken);
    }

    private async Task RevokeAllActiveTokensAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    )
    {
        var refreshToken = await _context
            .RefreshTokens.Where(x => x.UserId == userId)
            .ToListAsync(cancellationToken);

        foreach (var token in refreshToken)
        {
            token.DeletedAt = DateTimeOffset.UtcNow;
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    #endregion
}
