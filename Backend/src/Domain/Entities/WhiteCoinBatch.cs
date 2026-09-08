using MyTarotReader.Domain.Common;

namespace MyTarotReader.Domain.Entities;

/// <summary>
/// Represents a batch of white coins purchased by a user. Each batch has its own expiration date.
/// White coins are consumed on a first-in-first-out (FIFO) basis.
/// </summary>
public class WhiteCoinBatch : BaseEntity
{
    /// <summary>
    /// The identifier of the owning wallet.
    /// </summary>
    public Guid WalletId { get; set; }

    /// <summary>
    /// The total amount of white coins in this batch when purchased.
    /// </summary>
    public int Amount { get; set; }

    /// <summary>
    /// The remaining amount of white coins in this batch (not yet consumed or expired).
    /// </summary>
    public int RemainingAmount { get; set; } = 0;

    /// <summary>
    /// The expiration date and time of this white coin batch.
    /// </summary>
    public DateTimeOffset ExpiredAt { get; set; }

    #region Navigation Properties

    /// <summary>
    /// The owning wallet.
    /// </summary>
    public Wallet Wallet { get; set; } = null!;

    #endregion
}