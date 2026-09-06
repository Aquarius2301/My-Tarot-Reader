using Microsoft.EntityFrameworkCore;
using MyTarotReader.Application.Contracts.Persistence;
using MyTarotReader.Application.Contracts.Services;
using MyTarotReader.Application.Dtos;
using MyTarotReader.Application.Exceptions;

namespace MyTarotReader.Infrastructure.Services;

/// <inheritdoc />
/// <remarks>
/// All mutating operations use atomic SQL UPDATE via <see cref="EntityFrameworkQueryableExtensions.ExecuteUpdateAsync{TSource}"/>
/// to prevent race conditions when concurrent requests modify the same wallet.
/// </remarks>
public class WalletService(IAppDbContext context) : IWalletService
{
    private readonly IAppDbContext _context = context;

    /// <inheritdoc />
    public async Task<GetWalletResponse> GetWalletAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var wallet = await GetWalletOrThrowAsync(userId, cancellationToken);

        return new GetWalletResponse(wallet.WhiteCoin, wallet.RedCoin);
    }

    /// <inheritdoc />
    public async Task<AddWhiteCoinResponse> AddWhiteCoinAsync(
        Guid userId,
        int amount,
        CancellationToken cancellationToken = default)
    {
        if (amount <= 0)
            throw new BadRequestException(ErrorMessageCode.Wallet.InvalidAmount);

        var rowsAffected = await _context.Wallets
            .Where(w => w.UserId == userId)
            .ExecuteUpdateAsync(
                s => s.SetProperty(w => w.WhiteCoin, w => w.WhiteCoin + amount),
                cancellationToken);

        if (rowsAffected == 0)
            throw new NotFoundException(ErrorMessageCode.Wallet.WalletNotFound);

        var wallet = await _context.Wallets
            .AsNoTracking()
            .FirstAsync(w => w.UserId == userId, cancellationToken);

        return new AddWhiteCoinResponse(wallet.WhiteCoin, wallet.RedCoin);
    }

    /// <inheritdoc />
    public async Task<AddRedCoinResponse> AddRedCoinAsync(
        Guid userId,
        int amount,
        CancellationToken cancellationToken = default)
    {
        if (amount <= 0)
            throw new BadRequestException(ErrorMessageCode.Wallet.InvalidAmount);

        var rowsAffected = await _context.Wallets
            .Where(w => w.UserId == userId)
            .ExecuteUpdateAsync(
                s => s.SetProperty(w => w.RedCoin, w => w.RedCoin + amount),
                cancellationToken);

        if (rowsAffected == 0)
            throw new NotFoundException(ErrorMessageCode.Wallet.WalletNotFound);

        var wallet = await _context.Wallets
            .AsNoTracking()
            .FirstAsync(w => w.UserId == userId, cancellationToken);

        return new AddRedCoinResponse(wallet.WhiteCoin, wallet.RedCoin);
    }

    /// <inheritdoc />
    /// <remarks>
    /// Uses a single atomic SQL UPDATE with CASE expressions to enforce white-first deduction:
    /// <code>
    /// UPDATE Wallets
    /// SET WhiteCoin = CASE WHEN WhiteCoin >= @amount THEN WhiteCoin - @amount ELSE 0 END,
    ///     RedCoin   = CASE WHEN WhiteCoin >= @amount THEN RedCoin
    ///                     ELSE RedCoin - (@amount - WhiteCoin) END
    /// WHERE UserId = @userId AND (WhiteCoin + RedCoin) >= @amount
    /// </code>
    /// SQL Server row-level locking ensures concurrent requests serialize automatically.
    /// </remarks>
    public async Task<DeductCoinResponse> DeductCoinAsync(
        Guid userId,
        int amount,
        CancellationToken cancellationToken = default)
    {
        if (amount <= 0)
            throw new BadRequestException(ErrorMessageCode.Wallet.InvalidAmount);

        var rowsAffected = await _context.Wallets
            .Where(w => w.UserId == userId
                && (w.WhiteCoin + w.RedCoin) >= amount)
            .ExecuteUpdateAsync(
                s => s
                    .SetProperty(
                        w => w.WhiteCoin,
                        w => w.WhiteCoin >= amount
                            ? w.WhiteCoin - amount
                            : 0)
                    .SetProperty(
                        w => w.RedCoin,
                        w => w.WhiteCoin >= amount
                            ? w.RedCoin
                            : w.RedCoin - (amount - w.WhiteCoin)),
                cancellationToken);

        if (rowsAffected == 0)
        {
            // rowsAffected == 0 means either wallet not found or insufficient balance.
            // Distinguish by checking if the wallet exists.
            var walletExists = await _context.Wallets
                .AsNoTracking()
                .AnyAsync(w => w.UserId == userId, cancellationToken);

            if (!walletExists)
                throw new NotFoundException(ErrorMessageCode.Wallet.WalletNotFound);

            throw new BadRequestException(ErrorMessageCode.Wallet.InsufficientBalance);
        }

        var updated = await _context.Wallets
            .AsNoTracking()
            .FirstAsync(w => w.UserId == userId, cancellationToken);

        return new DeductCoinResponse(updated.WhiteCoin, updated.RedCoin);
    }

    /// <summary>
    /// Retrieves the wallet for the given user or throws <see cref="NotFoundException"/>.
    /// </summary>
    private async Task<Domain.Entities.Wallet> GetWalletOrThrowAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        return await _context.Wallets
            .FirstOrDefaultAsync(w => w.UserId == userId, cancellationToken)
            ?? throw new NotFoundException(ErrorMessageCode.Wallet.WalletNotFound);
    }
}
