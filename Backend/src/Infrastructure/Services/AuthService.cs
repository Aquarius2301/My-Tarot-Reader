using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Google.Apis.Auth;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MyTarotReader.Application.Contracts.Persistence;
using MyTarotReader.Application.Contracts.Services;
using MyTarotReader.Application.Dtos;
using MyTarotReader.Application.Exceptions;
using MyTarotReader.Application.Settings;
using MyTarotReader.Domain.Entities;

namespace MyTarotReader.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly GoogleSetting _googleSetting;
    private readonly JwtSetting _jwtSetting;
    private readonly WalletSetting _walletSetting;
    private readonly IAppDbContext _context;

    public AuthService(
        IOptions<GoogleSetting> googleSetting,
        IOptions<JwtSetting> jwtSetting,
        IOptions<WalletSetting> walletSetting,
        IAppDbContext context
    )
    {
        _googleSetting = googleSetting.Value;
        _jwtSetting = jwtSetting.Value;
        _walletSetting = walletSetting.Value;
        _context = context;
    }

    public async Task<GoogleLoginResult> GoogleLoginAsync(
        string credential,
        string deviceFingerprint,
        CancellationToken cancellationToken = default
    )
    {
        var googleClientId = _googleSetting.ClientId;

        var validationSettings = new GoogleJsonWebSignature.ValidationSettings
        {
            Audience = [googleClientId],
        };

        GoogleJsonWebSignature.Payload payload;

        try
        {
            payload = await GoogleJsonWebSignature.ValidateAsync(credential, validationSettings);
        }
        catch (InvalidJwtException)
        {
            throw new BadRequestException(ErrorMessageCode.Server.BadRequest);
        }

        // Reuse an existing account for this Google subject instead of duplicating on every login.
        var existing = await _context.Users.FirstOrDefaultAsync(
            u => u.ProviderKey == payload.Subject,
            cancellationToken
        );

        User userEntity;

        if (existing is null)
        {
            userEntity = new User
            {
                Email = payload.Email,
                FullName = payload.Name,
                ProviderKey = payload.Subject,
                Picture = payload.Picture,
            };
            _context.Users.Add(userEntity);
            CreateWalletForUser(userEntity.Id);
        }
        else
        {
            existing.Email = payload.Email;
            existing.FullName = payload.Name;
            existing.Picture = payload.Picture;

            userEntity = existing;
        }

        // revoke this device's existing active tokens so repeated logins on the same
        // machine don't pile up active rows. Other devices' sessions are preserved.
        var activeSameDevice = await _context
            .RefreshTokens.Where(r =>
                r.UserId == userEntity.Id
                && r.DeviceFingerprint == deviceFingerprint
                && r.DeletedAt == null
            )
            .ExecuteUpdateAsync(
                s => s.SetProperty(b => b.DeletedAt, DateTimeOffset.UtcNow),
                cancellationToken
            );

        var accessToken = GenerateJwtToken(userEntity.Id.ToString(), payload.Email);
        var refreshToken = CreateRefreshToken(userEntity.Id, deviceFingerprint);

        await _context.SaveChangesAsync(cancellationToken);

        return new GoogleLoginResult(accessToken, refreshToken);
    }

    private void CreateWalletForUser(Guid userId)
    {
        var wallet = new Wallet
        {
            UserId = userId,
            WhiteCoin = _walletSetting.InitialWhiteCoins,
            RedCoin = 0,
        };
        _context.Wallets.Add(wallet);
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
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Token == refreshToken, cancellationToken)
            ?? throw new UnauthorizedException(ErrorMessageCode.Auth.RefreshTokenInvalid);

        // if the token has been soft-deleted, the token may be stole and reused.
        if (existedToken.DeletedAt != null)
        {
            // Revoke all active tokens for the user to prevent further abuse.
            await RevokeAllActiveTokensAsync(existedToken.UserId, cancellationToken);

            throw new UnauthorizedException(ErrorMessageCode.Auth.RefreshTokenInvalid);
        }

        // if the user has been soft-deleted, the token may be stole and reused.
        if (existedToken.User is null || existedToken.User.DeletedAt != null)
        {
            await RevokeAllActiveTokensAsync(existedToken.UserId, cancellationToken);

            throw new UnauthorizedException(ErrorMessageCode.Auth.RefreshTokenInvalid);
        }

        // if the device fingerprint does not match, revoke all active tokens for the user to prevent further abuse.
        if (existedToken.DeviceFingerprint != deviceFingerprint)
        {
            await RevokeAllActiveTokensAsync(existedToken.UserId, cancellationToken);

            throw new UnauthorizedException(ErrorMessageCode.Auth.RefreshTokenInvalid);
        }

        // if the token has expired, soft-delete it to avoid reuse.
        if (existedToken.ExpiresAt < DateTimeOffset.UtcNow)
        {
            await _context
                .RefreshTokens.Where(x => x.Id == existedToken.Id && x.DeletedAt == null)
                .ExecuteUpdateAsync(
                    s => s.SetProperty(b => b.DeletedAt, DateTimeOffset.UtcNow),
                    cancellationToken
                );

            throw new UnauthorizedException(ErrorMessageCode.Auth.RefreshTokenInvalid);
        }

        // Atomically soft-delete this token ONLY if it's still active (DeletedAt == null).
        // If another concurrent request already rotated this token, rowsAffected == 0,
        // meaning this request is a replay/race — treat it as reuse.
        var rowsAffected = await _context
            .RefreshTokens.Where(x => x.Id == existedToken.Id && x.DeletedAt == null)
            .ExecuteUpdateAsync(
                s => s.SetProperty(b => b.DeletedAt, DateTimeOffset.UtcNow),
                cancellationToken
            );

        if (rowsAffected == 0)
        {
            // Lost the race — someone else already consumed this token.
            // This is a genuine reuse signal, revoke everything.
            await RevokeAllActiveTokensAsync(existedToken.UserId, cancellationToken);
            throw new UnauthorizedException(ErrorMessageCode.Auth.RefreshTokenInvalid);
        }

        var newAccessToken = GenerateJwtToken(
            existedToken.UserId.ToString(),
            existedToken.User.Email
        );

        var newRefreshToken = CreateRefreshToken(existedToken.UserId, deviceFingerprint);

        await _context.SaveChangesAsync(cancellationToken);

        return new GoogleLoginResult(newAccessToken, newRefreshToken);
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
        if (stored is not null && stored.DeletedAt is null)
        {
            stored.DeletedAt = DateTimeOffset.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<GetCurrentUserResponse> GetCurrentUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    )
    {
        var userEntity = await _context
            .Users.Include(u => u.Wallet)
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        // Treat a missing or soft-deleted account as an unauthenticated request.
        if (userEntity is null || userEntity.DeletedAt is not null)
        {
            throw new UnauthorizedException(ErrorMessageCode.Auth.Unauthorized);
        }

        var wallet = userEntity.Wallet;

        return new GetCurrentUserResponse(
            userEntity.Id,
            userEntity.Email,
            userEntity.FullName,
            userEntity.Picture,
            wallet.WhiteCoin,
            wallet.RedCoin,
            userEntity.Role
        );
    }

    private string CreateRefreshToken(Guid userId, string deviceFingerprint)
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);

        var token = Convert.ToBase64String(randomBytes);

        _context.RefreshTokens.Add(
            new RefreshToken
            {
                Token = token,
                UserId = userId,
                DeviceFingerprint = deviceFingerprint,
                ExpiresAt = DateTimeOffset.UtcNow.AddDays(_jwtSetting.RefreshTokenDurationDays),
            }
        );
        return token;
    }

    private async Task RevokeAllActiveTokensAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    )
    {
        await _context
            .RefreshTokens.Where(x => x.UserId == userId && x.DeletedAt == null)
            .ExecuteUpdateAsync(
                s => s.SetProperty(b => b.DeletedAt, DateTimeOffset.UtcNow),
                cancellationToken
            );
    }

    private string GenerateJwtToken(string userId, string email)
    {
        var secretKey = _jwtSetting.SecretKey;
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId),
            new Claim(JwtRegisteredClaimNames.Email, email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        var token = new JwtSecurityToken(
            issuer: _jwtSetting.Issuer,
            audience: _jwtSetting.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtSetting.AccessTokenDurationMinutes),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
