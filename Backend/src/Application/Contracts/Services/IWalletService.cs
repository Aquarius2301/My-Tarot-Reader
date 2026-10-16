using MyTarotReader.Domain.Enums;

namespace MyTarotReader.Application.Contracts.Services;

/// <summary>
/// Request to add coins (top-up) to a user's wallet.
/// </summary>
/// <param name="WhiteCoins">The number of white coins to add. Creates a new <see cref="MyTarotReader.Domain.Entities.WhiteCoinBatch"/>.</param>
/// <param name="RedCoins">The number of red coins to add. Added directly to the wallet balance.</param>
/// <param name="Type">The order type describing the transaction.</param>
public record AddCoinRequest(int WhiteCoins, int RedCoins, OrderType Type);

/// <summary>
/// Result of adding coins to a user's wallet.
/// </summary>
/// <param name="RedCoin">The updated red coin wallet balance.</param>
/// <param name="WhiteCoin">The updated white coin balance, derived from active, non-expired batches.</param>
public record AddCoinResult(int RedCoin, int WhiteCoin);

public interface IWalletService
{
    /// <summary>
    /// Adds white and/or red coins to a user's wallet and records the transaction as an order.
    /// </summary>
    /// <param name="userId">The authenticated user's ID.</param>
    /// <param name="request"><see cref="AddCoinRequest"/> containing the coin amounts and order type.</param>
    /// <exception cref="BadRequestException">Thrown when the request validation fails (amounts invalid).</exception>
    /// <exception cref="NotFoundException">Thrown when no wallet exists for the user.</exception>
    /// <returns>
    /// <see cref="AddCoinResult"/> with the updated red and white coin balances.
    /// </returns>
    /// <remarks>
    /// White coins are granted as a dated <see cref="MyTarotReader.Domain.Entities.WhiteCoinBatch"/> that expires after
    /// <c>WalletSetting.ExpireDays</c>, while red coins are added to the wallet balance directly.
    /// </remarks>
    Task<AddCoinResult> AddCoinAsync(
        Guid userId,
        AddCoinRequest request,
        CancellationToken cancellationToken = default
    );
}