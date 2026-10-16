using MediatR;

namespace MyTarotReader.Application.Features.Wallet;

public record GetWalletQuery(Guid UserId) : IRequest<GetWalletResult>;

/// <summary>
/// A single active white coin batch owned by the user.
/// </summary>
/// <param name="Id">The batch identifier.</param>
/// <param name="Amount">The original number of white coins granted in this batch.</param>
/// <param name="RemainingAmount">The number of white coins still usable from this batch.</param>
/// <param name="ExpiredAt">The date/time after which the remaining coins are forfeited.</param>
public record WhiteCoinBatchItem(
    Guid Id,
    int Amount,
    int RemainingAmount,
    DateTimeOffset ExpiredAt
);

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
