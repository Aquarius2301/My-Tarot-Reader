using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MyTarotReader.Application.Common.Exceptions;
using MyTarotReader.Application.Constants.Errors;
using MyTarotReader.Application.Persistence;
using MyTarotReader.Application.Services;
using MyTarotReader.Application.Settings;
using MyTarotReader.Domain.Entities;

namespace MyTarotReader.Application.Features.Auth;

public class RefreshCommandHandler(
    IAppDbContext context,
    IJwtTokenGeneratorService jwtTokenGeneratorService,
    IOptions<JwtSetting> jwtSetting
) : IRequestHandler<RefreshCommand, RefreshResult>
{
    public async Task<RefreshResult> Handle(
        RefreshCommand request,
        CancellationToken cancellationToken
    )
    {
        // get the token from the database, including the deleted token.
        var existedToken =
            await context
                .RefreshTokens.Include(x => x.User)
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(r => r.Token == request.RefreshToken, cancellationToken)
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
        if (existedToken.DeviceFingerprint != request.DeviceFingerprint)
        {
            await RevokeAllActiveTokensAsync(existedToken.UserId, cancellationToken);
            throw new UnauthorizedException(AuthErrorCode.InvalidRefreshToken);
        }

        var now = DateTimeOffset.UtcNow;

        // If the token is expired, just revoke it and ask the user to log in again.
        if (existedToken.ExpiresAt < now)
        {
            existedToken.DeletedAt = now;
            await context.SaveChangesAsync(cancellationToken);

            throw new UnauthorizedException(AuthErrorCode.InvalidRefreshToken);
        }

        // If the token is valid, generate a new access token and refresh token
        var newAccessToken = jwtTokenGeneratorService.GenerateAccessToken(existedToken.User);
        var newRefreshToken = jwtTokenGeneratorService.GenerateRefreshToken();

        existedToken.DeletedAt = now;

        AddRefreshToken(existedToken.User, request.DeviceFingerprint, newRefreshToken);

        await context.SaveChangesAsync(cancellationToken);

        return new RefreshResult(
            newAccessToken,
            newRefreshToken,
            jwtSetting.Value.AccessTokenDurationMinutes,
            jwtSetting.Value.RefreshTokenDurationDays
        );
    }

    #region Private Methods

    private void AddRefreshToken(User user, string deviceFingerprint, string token)
    {
        var refreshToken = new RefreshToken
        {
            UserId = user.Id,
            Token = token,
            DeviceFingerprint = deviceFingerprint,
            ExpiresAt = DateTimeOffset.UtcNow.AddDays(jwtSetting.Value.RefreshTokenDurationDays),
        };

        context.RefreshTokens.Add(refreshToken);
    }

    private async Task RevokeAllActiveTokensAsync(Guid userId, CancellationToken cancellationToken)
    {
        var refreshToken = await context
            .RefreshTokens.Where(x => x.UserId == userId)
            .ToListAsync(cancellationToken);

        foreach (var token in refreshToken)
        {
            token.DeletedAt = DateTimeOffset.UtcNow;
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    #endregion
}
