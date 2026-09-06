using MyTarotReader.Application.Dtos;
using MyTarotReader.Application.Exceptions;

namespace MyTarotReader.Application.Contracts.Services;

/// <summary>
/// Manages the user's wallet coin balances (WhiteCoin and RedCoin).
/// </summary>
public interface IWalletService
{
    /// <summary>
    /// Retrieves the current WhiteCoin and RedCoin balances for the specified user.
    /// </summary>
    /// <param name="userId">The identifier of the user whose wallet to retrieve.</param>
    /// <param name="cancellationToken">Token to observe for task cancellation.</param>
    /// <returns>A <see cref="GetWalletResponse"/> containing both coin balances.</returns>
    /// <exception cref="NotFoundException">Thrown when no wallet exists for the user.</exception>
    Task<GetWalletResponse> GetWalletAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Increases the user's WhiteCoin balance by the specified amount.
    /// </summary>
    /// <param name="userId">The identifier of the user whose wallet to modify.</param>
    /// <param name="amount">The number of white coins to add. Must be positive.</param>
    /// <param name="cancellationToken">Token to observe for task cancellation.</param>
    /// <returns>An <see cref="AddWhiteCoinResponse"/> with the updated balances.</returns>
    /// <exception cref="NotFoundException">Thrown when no wallet exists for the user.</exception>
    /// <exception cref="BadRequestException">Thrown when the amount is not a positive integer.</exception>
    Task<AddWhiteCoinResponse> AddWhiteCoinAsync(
        Guid userId,
        int amount,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Increases the user's RedCoin balance by the specified amount.
    /// </summary>
    /// <param name="userId">The identifier of the user whose wallet to modify.</param>
    /// <param name="amount">The number of red coins to add. Must be positive.</param>
    /// <param name="cancellationToken">Token to observe for task cancellation.</param>
    /// <returns>An <see cref="AddRedCoinResponse"/> with the updated balances.</returns>
    /// <exception cref="NotFoundException">Thrown when no wallet exists for the user.</exception>
    /// <exception cref="BadRequestException">Thrown when the amount is not a positive integer.</exception>
    Task<AddRedCoinResponse> AddRedCoinAsync(
        Guid userId,
        int amount,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Decreases the user's total coin balance by the specified amount.
    /// Coins are consumed from WhiteCoin first; any remainder is taken from RedCoin.
    /// Throws when the combined balance is less than the requested amount.
    /// </summary>
    /// <param name="userId">The identifier of the user whose wallet to modify.</param>
    /// <param name="amount">The total number of coins to deduct. Must be positive.</param>
    /// <param name="cancellationToken">Token to observe for task cancellation.</param>
    /// <returns>A <see cref="DeductCoinResponse"/> with the updated balances.</returns>
    /// <exception cref="NotFoundException">Thrown when no wallet exists for the user.</exception>
    /// <exception cref="BadRequestException">Thrown when the amount is not positive or the balance is insufficient.</exception>
    Task<DeductCoinResponse> DeductCoinAsync(
        Guid userId,
        int amount,
        CancellationToken cancellationToken = default);
}
