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
public class WalletService(
    IAppDbContext context,
    IOptions<WalletSetting> walletSetting
) : IWalletService
{
    private readonly IAppDbContext _context = context;
    private readonly WalletSetting _walletSetting = walletSetting.Value;

    /// <inheritdoc />
    public async Task<GetWalletResponse> GetWalletAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    )
    {
        var wallet = await _context.Wallets
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.UserId == userId, cancellationToken);

        if (wallet is null)
        {
            throw new NotFoundException(ErrorMessageCode.Server.NotFound);
        }

        return new GetWalletResponse(wallet.WhiteCoin, wallet.RedCoin);
    }

    /// <inheritdoc />
    public async Task<AddWhiteCoinResponse> AddWhiteCoinAsync(
        Guid userId,
        int amount,
        CancellationToken cancellationToken = default
    )
    {
        if (amount <= 0)
        {
            throw new BadRequestException(ErrorMessageCode.Server.BadRequest);
        }

        var wallet = await _context.Wallets
            .Include(w => w.WhiteCoinBatches)
            .FirstOrDefaultAsync(w => w.UserId == userId, cancellationToken);

        if (wallet is null)
        {
            throw new NotFoundException(ErrorMessageCode.Server.NotFound);
        }

        var now = DateTimeOffset.UtcNow;
        var expiredAt = now.AddDays(_walletSetting.ExpireDays);

        // Create new white coin batch
        var batch = new WhiteCoinBatch
        {
            WalletId = wallet.Id,
            Amount = amount,
            RemainingAmount = amount,
            ExpiredAt = expiredAt,
        };

        _context.WhiteCoinBatches.Add(batch);

        // Create transaction record for audit trail
        var transaction = new Transaction
        {
            UserId = userId,
            Amount = amount,
            Type = TransactionType.TopUp,
            Description = $"Add {amount} white coins",
            TransactionDetails =
            [
                new TransactionDetail
                {
                    Amount = amount,
                    WhiteCoinBatchId = batch.Id,
                },
            ],
        };

        _context.Transactions.Add(transaction);

        // Update wallet UpdatedAt
        wallet.UpdatedAt = now;

        await _context.SaveChangesAsync(cancellationToken);

        return new AddWhiteCoinResponse(wallet.WhiteCoin, wallet.RedCoin);
    }

    /// <inheritdoc />
    public async Task<AddRedCoinResponse> AddRedCoinAsync(
        Guid userId,
        int amount,
        CancellationToken cancellationToken = default
    )
    {
        if (amount <= 0)
        {
            throw new BadRequestException(ErrorMessageCode.Server.BadRequest);
        }

        var wallet = await _context.Wallets
            .FirstOrDefaultAsync(w => w.UserId == userId, cancellationToken);

        if (wallet is null)
        {
            throw new NotFoundException(ErrorMessageCode.Server.NotFound);
        }

        var now = DateTimeOffset.UtcNow;

        // Atomically update RedCoin and UpdatedAt
        await _context.Wallets
            .Where(w => w.UserId == userId)
            .ExecuteUpdateAsync(
                s => s
                    .SetProperty(w => w.RedCoin, w => w.RedCoin + amount)
                    .SetProperty(w => w.UpdatedAt, now),
                cancellationToken
            );

        // Create transaction record for audit trail
        var transaction = new Transaction
        {
            UserId = userId,
            Amount = amount,
            Type = TransactionType.TopUp,
            Description = $"Add {amount} red coins",
            TransactionDetails =
            [
                new TransactionDetail
                {
                    Amount = amount,
                },
            ],
        };

        _context.Transactions.Add(transaction);

        await _context.SaveChangesAsync(cancellationToken);

        // Reload to get updated WhiteCoin (computed from batches)
        var updatedWallet = await _context.Wallets
            .AsNoTracking()
            .FirstAsync(w => w.UserId == userId, cancellationToken);

        return new AddRedCoinResponse(updatedWallet.WhiteCoin, updatedWallet.RedCoin);
    }

    /// <inheritdoc />
    public async Task<DeductCoinResponse> DeductCoinAsync(
        Guid userId,
        int amount,
        CancellationToken cancellationToken = default
    )
    {
        if (amount <= 0)
        {
            throw new BadRequestException(ErrorMessageCode.Server.BadRequest);
        }

        // Load wallet with active white coin batches (FIFO order: oldest expiry first)
        var wallet = await _context.Wallets
            .Include(w => w.WhiteCoinBatches.Where(b =>
                b.RemainingAmount > 0 && b.ExpiredAt > DateTimeOffset.UtcNow
            ).OrderBy(b => b.ExpiredAt).ThenBy(b => b.CreatedAt))
            .FirstOrDefaultAsync(w => w.UserId == userId, cancellationToken);

        if (wallet is null)
        {
            throw new NotFoundException(ErrorMessageCode.Server.NotFound);
        }

        var totalAvailable = wallet.WhiteCoin + wallet.RedCoin;
        if (totalAvailable < amount)
        {
            throw new BadRequestException(ErrorMessageCode.Wallet.InsufficientBalance);
        }

        var now = DateTimeOffset.UtcNow;
        var remainingToDeduct = amount;
        var transactionDetails = new List<TransactionDetail>();

        // Deduct from WhiteCoinBatches FIFO
        foreach (var batch in wallet.WhiteCoinBatches)
        {
            if (remainingToDeduct <= 0)
            {
                break;
            }

            var deductFromBatch = Math.Min(batch.RemainingAmount, remainingToDeduct);

            // Atomically update batch RemainingAmount
            var rowsAffected = await _context.WhiteCoinBatches
                .Where(b => b.Id == batch.Id && b.RemainingAmount >= deductFromBatch)
                .ExecuteUpdateAsync(
                    s => s.SetProperty(b => b.RemainingAmount, b => b.RemainingAmount - deductFromBatch),
                    cancellationToken
                );

            if (rowsAffected == 0)
            {
                // Concurrent modification - reload and retry (or throw)
                throw new ConflictException(ErrorMessageCode.Server.Conflict);
            }

            transactionDetails.Add(new TransactionDetail
            {
                Amount = -deductFromBatch,
                WhiteCoinBatchId = batch.Id,
            });

            remainingToDeduct -= deductFromBatch;
        }

        // Deduct remaining from RedCoin if needed
        if (remainingToDeduct > 0)
        {
            var rowsAffected = await _context.Wallets
                .Where(w => w.UserId == userId && w.RedCoin >= remainingToDeduct)
                .ExecuteUpdateAsync(
                    s => s
                        .SetProperty(w => w.RedCoin, w => w.RedCoin - remainingToDeduct)
                        .SetProperty(w => w.UpdatedAt, now),
                    cancellationToken
                );

            if (rowsAffected == 0)
            {
                throw new ConflictException(ErrorMessageCode.Server.Conflict);
            }

            transactionDetails.Add(new TransactionDetail
            {
                Amount = -remainingToDeduct,
            });

            remainingToDeduct = 0;
        }

        // Create transaction record for audit trail
        var transaction = new Transaction
        {
            UserId = userId,
            Amount = -amount,
            Type = TransactionType.Spend,
            Description = $"Spend {amount} coins",
            TransactionDetails = transactionDetails,
        };

        _context.Transactions.Add(transaction);

        await _context.SaveChangesAsync(cancellationToken);

        // Reload to get updated balances
        var updatedWallet = await _context.Wallets
            .AsNoTracking()
            .FirstAsync(w => w.UserId == userId, cancellationToken);

        return new DeductCoinResponse(updatedWallet.WhiteCoin, updatedWallet.RedCoin);
    }
}