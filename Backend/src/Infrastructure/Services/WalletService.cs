using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MyTarotReader.Application.Common.Exceptions;
using MyTarotReader.Application.Constants.Errors;
using MyTarotReader.Application.Persistence;
using MyTarotReader.Application.Services;
using MyTarotReader.Application.Settings;
using MyTarotReader.Domain.Entities;
using MyTarotReader.Domain.Enums;

namespace MyTarotReader.Infrastructure.Services;

public class WalletService(IAppDbContext context, IOptions<WalletSetting> walletSetting)
    : IWalletService
{
    private readonly WalletSetting _walletSetting = walletSetting.Value;

    public async Task AddCoinsAsync(
        Guid userId,
        int redCoins,
        int whiteCoins,
        OrderType orderType,
        CancellationToken cancellationToken
    )
    {
        if (redCoins < 0 || whiteCoins < 0 || (redCoins == 0 && whiteCoins == 0))
        {
            throw new ArgumentException(WalletErrorCode.InsufficientCoins);
        }

        var user =
            context
                .Users.Where(u => u.Id == userId)
                .Select(u => new { u.Id, u.Wallet })
                .FirstOrDefault()
            ?? throw new NotFoundException(WalletErrorCode.WalletNotFound);

        // Add red coins to the user's wallet
        user.Wallet.RedCoin += redCoins;

        // Add white coins to the user's wallet as a new WhiteCoinBatch
        if (whiteCoins > 0)
        {
            context.WhiteCoinBatches.Add(
                new WhiteCoinBatch
                {
                    WalletId = user.Wallet.Id,
                    Amount = whiteCoins,
                    RemainingAmount = whiteCoins,
                    ExpiredAt = DateTimeOffset.UtcNow.AddDays(_walletSetting.ExpireDays),
                    CreatedAt = DateTimeOffset.UtcNow,
                }
            );
        }

        // Create a new order to record the coin addition
        var order = new Order
        {
            UserId = user.Id,
            RedCoins = redCoins,
            WhiteCoins = whiteCoins,
            Type = orderType,
            Description = $"Added {redCoins} red coins and {whiteCoins} white coins.",
            CreatedAt = DateTimeOffset.UtcNow,
        };

        context.Orders.Add(order);

        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeductCoinsAsync(
        Guid userId,
        int redCoins,
        int whiteCoins,
        OrderType orderType,
        CancellationToken cancellationToken
    )
    {
        if (redCoins < 0 || whiteCoins < 0 || (redCoins == 0 && whiteCoins == 0))
        {
            throw new ArgumentException(WalletErrorCode.InsufficientCoins);
        }

        var user =
            context
                .Users.Where(u => u.Id == userId)
                .Select(u => new
                {
                    u.Id,
                    u.Wallet,
                    u.Wallet.WhiteCoinBatches,
                })
                .FirstOrDefault()
            ?? throw new NotFoundException(WalletErrorCode.WalletNotFound);

        // Check if the user has enough red coins
        if (user.Wallet.RedCoin < redCoins)
        {
            throw new ArgumentException(WalletErrorCode.InsufficientRedCoins);
        }

        // Check if the user has enough white coins
        if (
            user.WhiteCoinBatches.Where(b =>
                    b.ExpiredAt > DateTimeOffset.UtcNow && b.RemainingAmount > 0
                )
                .Sum(b => b.Amount) < whiteCoins
        )
        {
            throw new ArgumentException(WalletErrorCode.InsufficientWhiteCoins);
        }

        // Deduct red coins from the user's wallet
        user.Wallet.RedCoin -= redCoins;

        // Deduct white coins from the user's wallet
        var whiteCoinBatches = user
            .Wallet.WhiteCoinBatches.Where(b =>
                b.ExpiredAt > DateTimeOffset.UtcNow && b.RemainingAmount > 0
            )
            .OrderBy(x => x.CreatedAt)
            .ToList();

        var remainingWhiteCoins = whiteCoins;

        foreach (var batch in whiteCoinBatches)
        {
            if (remainingWhiteCoins <= 0)
                break;
            if (batch.RemainingAmount > 0)
            {
                var deduction = Math.Min(remainingWhiteCoins, batch.RemainingAmount);
                batch.RemainingAmount -= deduction;
                remainingWhiteCoins -= deduction;
            }
        }

        // Create a new order to record the coin deduction
        var order = new Order
        {
            UserId = user.Id,
            RedCoins = redCoins,
            WhiteCoins = whiteCoins,
            Type = orderType,
            Description = $"Deducted {redCoins} red coins and {whiteCoins} white coins.",
            CreatedAt = DateTimeOffset.UtcNow,
        };

        context.Orders.Add(order);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<UserCoins> GetCoinsAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user =
            await context
                .Users.Where(u => u.Id == userId)
                .Select(u => new UserCoins(
                    u.Wallet.RedCoin,
                    u.Wallet.WhiteCoinBatches.Where(b =>
                            b.ExpiredAt > DateTimeOffset.UtcNow && b.RemainingAmount > 0
                        )
                        .Sum(b => b.Amount)
                ))
                .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException(WalletErrorCode.WalletNotFound);

        return new UserCoins(user.RedCoins, user.WhiteCoins);
    }
}
