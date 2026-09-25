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

/// <summary>
/// Request to deduct coins from a user's wallet.
/// </summary>
/// <param name="Amount">The number of coins to deduct (white coins only).</param>
/// <param name="Type">The order type describing the transaction.</param>
public record DeductCoinRequest(int Amount, OrderType Type);

/// <summary>
/// Result of deducting coins from a user's wallet.
/// </summary>
/// <param name="WhiteCoin">The updated white coin balance, derived from active, non-expired batches.</param>
public record DeductCoinResult(int WhiteCoin);

/// <summary>
/// Result of querying a user's wallet balance.
/// </summary>
/// <param name="WhiteCoin">The white coin balance, derived from active, non-expired batches.</param>
/// <param name="RedCoin">The red coin wallet balance.</param>
public record GetWalletBalanceResult(int WhiteCoin, int RedCoin);

/// <summary>
/// A single active white coin batch owned by the user.
/// </summary>
/// <param name="Id">The batch identifier.</param>
/// <param name="Amount">The original number of white coins granted in this batch.</param>
/// <param name="RemainingAmount">The number of white coins still usable from this batch.</param>
/// <param name="ExpiredAt">The date/time after which the remaining coins are forfeited.</param>
public record WhiteCoinBatchItem(Guid Id, int Amount, int RemainingAmount, DateTimeOffset ExpiredAt);

/// <summary>
/// Result of querying a user's wallet balance together with their white coin batches.
/// </summary>
/// <param name="WhiteCoin">The white coin balance, derived from active, non-expired batches.</param>
/// <param name="RedCoin">The red coin wallet balance.</param>
/// <param name="WhiteCoinBatches">The active white coin batches ordered by expiry (soonest first).</param>
public record GetWalletResult(
    int WhiteCoin,
    int RedCoin,
    List<WhiteCoinBatchItem> WhiteCoinBatches
);

/// <summary>
/// Request to convert red coins into white coins.
/// </summary>
/// <param name="RedCoins">The number of red coins to convert. <c>1</c> red coin yields <c>2</c> white coins.</param>
public record ConvertRedToWhiteRequest(int RedCoins);

/// <summary>
/// Result of converting red coins into white coins.
/// </summary>
/// <param name="RedCoin">The updated red coin wallet balance.</param>
/// <param name="WhiteCoin">The updated white coin balance, derived from active, non-expired batches.</param>
public record ConvertRedToWhiteResult(int RedCoin, int WhiteCoin);

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

    /// <summary>
    /// Returns the current white and red coin balances for a user.
    /// </summary>
    /// <param name="userId">The authenticated user's ID.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <exception cref="NotFoundException">Thrown when no wallet exists for the user.</exception>
    /// <returns><see cref="GetWalletBalanceResult"/> with the current balances.</returns>
    Task<GetWalletBalanceResult> GetBalanceAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Deducts white coins from a user's wallet, spending the oldest active batch first,
    /// and records the transaction as an order.
    /// </summary>
    /// <param name="userId">The authenticated user's ID.</param>
    /// <param name="request"><see cref="DeductCoinRequest"/> containing the amount and order type.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <exception cref="BadRequestException">Thrown when the request validation fails or the user does not have enough white coins.</exception>
    /// <exception cref="NotFoundException">Thrown when no wallet exists for the user.</exception>
    /// <returns><see cref="DeductCoinResult"/> with the updated white coin balance.</returns>
    Task<DeductCoinResult> DeductCoinAsync(
        Guid userId,
        DeductCoinRequest request,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Returns the current white and red coin balances together with the active
    /// white coin batches ordered by expiry (soonest first).
    /// </summary>
    /// <param name="userId">The authenticated user's ID.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <exception cref="NotFoundException">Thrown when no wallet exists for the user.</exception>
    /// <returns><see cref="GetWalletResult"/> with the balances and active white coin batches.</returns>
    Task<GetWalletResult> GetWalletAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Converts red coins into white coins (1 red = 2 white) as a new dated batch
    /// and records the transaction as an order.
    /// </summary>
    /// <param name="userId">The authenticated user's ID.</param>
    /// <param name="request"><see cref="ConvertRedToWhiteRequest"/> containing the number of red coins to convert.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <exception cref="BadRequestException">Thrown when the request validation fails or the user does not have enough red coins.</exception>
    /// <exception cref="NotFoundException">Thrown when no wallet exists for the user.</exception>
    /// <returns><see cref="ConvertRedToWhiteResult"/> with the updated balances.</returns>
    Task<ConvertRedToWhiteResult> ConvertRedToWhiteAsync(
        Guid userId,
        ConvertRedToWhiteRequest request,
        CancellationToken cancellationToken = default
    );
}