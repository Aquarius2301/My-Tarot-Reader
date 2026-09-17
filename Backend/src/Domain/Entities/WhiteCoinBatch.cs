using MyTarotReader.Domain.Common;

namespace MyTarotReader.Domain.Entities;

public class WhiteCoinBatch : BaseEntity
{
    public Guid WalletId { get; set; }

    /// <summary>
    /// The original amount of white coins granted in this batch.
    /// </summary>
    public int Amount { get; set; }

    /// <summary>
    /// The unused amount remaining in this batch. Decreases as coins are spent
    /// (oldest batch consumed first) until it reaches zero or the batch expires.
    /// </summary>
    public int RemainingAmount { get; set; } = 0;

    /// <summary>
    /// The date and time after which any <see cref="RemainingAmount"/> in this batch
    /// is forfeited and no longer usable.
    /// </summary>
    public DateTimeOffset ExpiredAt { get; set; }

    #region Navigation Properties

    public Wallet Wallet { get; set; } = null!;

    #endregion
}
