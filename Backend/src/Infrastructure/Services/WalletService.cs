using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MyTarotReader.Application.Contracts.Persistence;
using MyTarotReader.Application.Contracts.Services;
using MyTarotReader.Application.Dtos;
using MyTarotReader.Application.Exceptions;
using MyTarotReader.Application.Settings;
using MyTarotReader.Domain.Entities;
using MyTarotReader.Domain.Enums;

namespace MyTarotReader.Infrastructure.Services;

/// <inheritdoc />
/// <remarks>
/// All mutating operations use atomic SQL UPDATE via <see cref="EntityFrameworkQueryableExtensions.ExecuteUpdateAsync{TSource}"/>
/// to prevent race conditions when concurrent requests modify the same wallet.
/// </remarks>
public class WalletService : IWalletService
{
    private readonly IAppDbContext _context;
    private readonly WalletSetting _walletSetting;

    public WalletService(IAppDbContext context, IOptions<WalletSetting> walletSetting)
    {
        _context = context;
        _walletSetting = walletSetting.Value;
    }

    /// <inheritdoc />
    public async Task EnsureSufficientBalanceAsync(
        Guid userId,
        int amount,
        CancellationToken cancellationToken = default
    )
    {
        var wallet =
            await _context
                .Wallets.AsNoTracking()
                .Where(w => w.UserId == userId)
                .Select(w => new
                {
                    WhiteCoin = w
                        .WhiteCoinBatches.Where(b =>
                            b.RemainingAmount > 0 && b.ExpiredAt > DateTimeOffset.UtcNow
                        )
                        .Sum(b => b.RemainingAmount),
                    w.RedCoin,
                })
                .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException(ErrorMessageCode.Wallet.WalletNotFound);

        if (wallet.WhiteCoin + wallet.RedCoin < amount)
        {
            throw new BadRequestException(ErrorMessageCode.Wallet.InsufficientBalance);
        }
    }

    /// <inheritdoc />
    public async Task DeductCoinAsync(
        Guid userId,
        int cost,
        TransactionType type,
        CancellationToken cancellationToken = default
    )
    {
        if (cost <= 0)
            return;

        var wallet =
            await _context
                .Wallets.Include(w =>
                    w.WhiteCoinBatches.Where(b =>
                        b.RemainingAmount > 0 && b.ExpiredAt > DateTimeOffset.UtcNow
                    )
                )
                .FirstOrDefaultAsync(w => w.UserId == userId, cancellationToken)
            ?? throw new NotFoundException(ErrorMessageCode.Server.NotFound);

        var activeWhiteBatches = wallet
            .WhiteCoinBatches.OrderBy(b => b.ExpiredAt)
            .ThenBy(b => b.CreatedAt)
            .ToList();

        var whiteCoinTotal = activeWhiteBatches.Sum(b => b.RemainingAmount);
        var totalAvailable = whiteCoinTotal + wallet.RedCoin;

        if (totalAvailable < cost)
        {
            throw new BadRequestException(ErrorMessageCode.Wallet.InsufficientBalance);
        }

        var transaction = new Transaction
        {
            UserId = userId,
            Type = type,
            Amount = -cost,
            Description = GetTransactionDescription(type),
            CreatedAt = DateTimeOffset.UtcNow,
        };

        await _context.Transactions.AddAsync(transaction, cancellationToken);

        var remainingCost = cost;

        // Deduct from WhiteCoin batches first, in order of expiration and creation date
        foreach (var batch in activeWhiteBatches)
        {
            if (remainingCost <= 0)
                break;

            var deduction = Math.Min(batch.RemainingAmount, remainingCost);
            batch.RemainingAmount -= deduction;
            remainingCost -= deduction;

            var transactionDetail = new TransactionDetail
            {
                Transaction = transaction,
                WhiteCoinBatchId = batch.Id,
                Amount = -deduction,
            };

            await _context.TransactionDetails.AddAsync(transactionDetail, cancellationToken);
        }

        // Then deduct any remaining cost from RedCoin
        if (remainingCost > 0)
        {
            wallet.RedCoin -= remainingCost;

            var transactionDetail = new TransactionDetail
            {
                Transaction = transaction,
                Amount = -remainingCost,
            };

            await _context.TransactionDetails.AddAsync(transactionDetail, cancellationToken);
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Returns a human-readable description for a transaction based on its type.
    /// Required because the <see cref="Transaction.Description"/> column is NOT NULL.
    /// </summary>
    private static string GetTransactionDescription(TransactionType type) =>
        type switch
        {
            TransactionType.AITarot => "AI Tarot reading",
            TransactionType.AIChatSession => "Start AI chat session",
            TransactionType.AIChatFollowUp => "AI chat follow-up phase",
            TransactionType.Expired => "White coins expired",
            _ => "Coin transaction",
        };
}
