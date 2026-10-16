using System;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MyTarotReader.Application.Common.Exceptions;
using MyTarotReader.Application.Constants.Errors;
using MyTarotReader.Application.Persistence;
using MyTarotReader.Application.Services;
using MyTarotReader.Application.Settings;
using MyTarotReader.Domain.Entities;
using MyTarotReader.Domain.Enums;
using StreakEntity = MyTarotReader.Domain.Entities.Streak;
using WalletEntity = MyTarotReader.Domain.Entities.Wallet;

namespace MyTarotReader.Application.Features.Auth;

public class GoogleLoginCommandHandler(
    IAppDbContext context,
    IJwtTokenGeneratorService jwtTokenGeneratorService,
    IGoogleAuthService googleAuthService,
    IEmailQueueService emailQueueService,
    IOptions<WalletSetting> walletSetting,
    IOptions<JwtSetting> jwtSetting
) : IRequestHandler<GoogleLoginCommand, GoogleLoginResult>
{
    public async Task<GoogleLoginResult> Handle(
        GoogleLoginCommand request,
        CancellationToken cancellationToken
    )
    {
        var payload =
            await googleAuthService.ValidateAsync(request.Credential)
            ?? throw new BadRequestException(AuthErrorCode.InvalidKeyCredential);

        // Check if the user already exists in the database based on the provider key (Google ID)
        var user = await context.Users.FirstOrDefaultAsync(
            u => u.ProviderKey == payload.ProviderKey,
            cancellationToken
        );

        var isNewUser = false;

        if (user == null)
        {
            isNewUser = true;

            user = new User
            {
                FullName = payload.Name,
                Email = payload.Email,
                Picture = payload.Picture,
                ProviderKey = payload.ProviderKey,
                Role = UserRole.Registered,
            };

            context.Users.Add(user);

            var whiteCoinBatch = new WhiteCoinBatch
            {
                Amount = walletSetting.Value.InitialWhiteCoins,
                RemainingAmount = walletSetting.Value.InitialWhiteCoins,
                ExpiredAt = DateTimeOffset.UtcNow.AddDays(walletSetting.Value.ExpireDays),
            };

            var wallet = new WalletEntity
            {
                UserId = user.Id,
                WhiteCoinBatches = [whiteCoinBatch],
            };
            context.Wallets.Add(wallet);

            context.Orders.Add(
                new Order
                {
                    UserId = user.Id,
                    RedCoins = 0,
                    WhiteCoins = walletSetting.Value.InitialWhiteCoins,
                    Description = "First login bonus",
                    Type = OrderType.FirstLogin,
                }
            );

            context.Streaks.Add(new StreakEntity { UserId = user.Id });
        }
        else
        {
            // Sync user information with the latest data from Google
            user.FullName = payload.Name;
            user.Picture = payload.Picture;
            user.ProviderKey = payload.ProviderKey;
        }

        var accessToken = jwtTokenGeneratorService.GenerateAccessToken(user);
        var refreshToken = jwtTokenGeneratorService.GenerateRefreshToken();

        // Check if a refresh token already exists for this user and device fingerprint
        var existingToken = await context.RefreshTokens.FirstOrDefaultAsync(
            rt => rt.UserId == user.Id && rt.DeviceFingerprint == request.DeviceFingerprint,
            cancellationToken
        );

        if (existingToken != null)
        {
            existingToken.DeletedAt = DateTimeOffset.UtcNow;
        }

        AddRefreshToken(user, request.DeviceFingerprint, refreshToken);

        await context.SaveChangesAsync(cancellationToken);

        if (isNewUser)
        {
            await emailQueueService.EnqueueAsync(
                new WelcomeEmailMessage(user.Email, user.FullName, request.Locale),
                CancellationToken.None
            );
        }

        return new GoogleLoginResult(
            accessToken,
            refreshToken,
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

    #endregion
}
