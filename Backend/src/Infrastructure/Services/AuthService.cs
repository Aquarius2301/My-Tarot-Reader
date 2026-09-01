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
            .ToListAsync(cancellationToken);

        foreach (var token in activeSameDevice)
        {
            token.DeletedAt = DateTime.UtcNow;
        }

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
        var stored =
            await _context.RefreshTokens.FirstOrDefaultAsync(
                r => r.Token == refreshToken,
                cancellationToken
            ) ?? throw new UnauthorizedException(ErrorMessageCode.Auth.RefreshTokenInvalid);

        // Reuse detection: a consumed token being replayed means it was likely stolen.
        // Revoke the whole active token family for the user and reject.
        if (stored.DeletedAt is not null)
        {
            var family = await _context
                .RefreshTokens.Where(r => r.UserId == stored.UserId && r.DeletedAt == null)
                .ToListAsync(cancellationToken);
            foreach (var token in family)
            {
                token.DeletedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync(cancellationToken);
            throw new UnauthorizedException(ErrorMessageCode.Auth.RefreshTokenInvalid);
        }

        // a token copied to a different machine yields a different
        // visitorId, so reject instead of rotating it.
        if (stored.DeviceFingerprint != deviceFingerprint)
        {
            throw new UnauthorizedException(ErrorMessageCode.Auth.RefreshTokenInvalid);
        }

        if (stored.ExpiresAt < DateTime.UtcNow)
        {
            stored.DeletedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
            throw new UnauthorizedException(ErrorMessageCode.Auth.RefreshTokenExpired);
        }

        // Rotate: revoke the presented token and issue a fresh pair (same device).
        stored.DeletedAt = DateTime.UtcNow;

        var dbUser =
            await _context
                .Users.AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == stored.UserId, cancellationToken)
            ?? throw new UnauthorizedException(ErrorMessageCode.Auth.RefreshTokenInvalid);

        var newAccessToken = GenerateJwtToken(dbUser.Id.ToString(), dbUser.Email);
        var newRefreshToken = CreateRefreshToken(dbUser.Id, deviceFingerprint);

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
            stored.DeletedAt = DateTime.UtcNow;
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
                ExpiresAt = DateTime.UtcNow.AddDays(_jwtSetting.RefreshTokenDurationDays),
            }
        );
        return token;
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
