using MyTarotReader.Application.Dtos;
using MyTarotReader.Application.Exceptions;
using MyTarotReader.Domain.Enums;

namespace MyTarotReader.Application.Contracts.Services;

/// <summary>
/// Manages the user's wallet coin balances (WhiteCoin and RedCoin).
/// </summary>
public interface IWalletService
{
    /// <summary>
    /// Ensures the user's combined coin balance (WhiteCoin + RedCoin) is at least
    /// <paramref name="amount"/>. Used to fail fast before an expensive external call,
    /// without holding a database transaction open across it.
    /// </summary>
    /// <param name="userId">The identifier of the user whose wallet to check.</param>
    /// <param name="amount">The minimum required coin amount. Must be positive.</param>
    /// <param name="cancellationToken">Token to observe for task cancellation.</param>
    /// <returns>Throw exception if the balance is insufficient.</returns>
    /// <exception cref="NotFoundException">Thrown when no wallet exists for the user.</exception>
    /// <exception cref="BadRequestException">Thrown when the amount is not positive or the balance is insufficient.</exception>
    Task EnsureSufficientBalanceAsync(
        Guid userId,
        int amount,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Deducts the specified <paramref name="cost"/> from the user's wallet, prioritizing WhiteCoin over RedCoin. Throws if the user has insufficient balance.
    /// </summary>
    /// <param name="userId">The identifier of the user whose wallet to deduct from.</param>
    /// <param name="cost">The amount to deduct. Must be positive.</param>
    /// <param name="type">The transaction type recorded in the ledger (e.g. <see cref="TransactionType.AITarot"/> or <see cref="TransactionType.AIChatSession"/>).</param>
    /// <param name="cancellationToken">Token to observe for task cancellation.</param>
    /// <returns>Throws exception if the balance is insufficient.</returns>
    /// <exception cref="NotFoundException">Thrown when no wallet exists for the user.</exception>
    /// <exception cref="BadRequestException">Thrown when the cost is not positive or the balance is insufficient.</exception>
    Task DeductCoinAsync(
        Guid userId,
        int cost,
        TransactionType type,
        CancellationToken cancellationToken = default
    );
}
