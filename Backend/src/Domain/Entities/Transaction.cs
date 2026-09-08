using MyTarotReader.Domain.Common;
using MyTarotReader.Domain.Enums;

namespace MyTarotReader.Domain.Entities;

/// <summary>
/// Represents a transaction record for a user's coin balance changes.
/// Each transaction can contain multiple details (line items).
/// </summary>
public class Transaction : BaseEntity
{
    /// <summary>
    /// The identifier of the user who owns this transaction.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// The total amount of coins affected by this transaction (positive for credit, negative for debit).
    /// </summary>
    public int Amount { get; set; }

    /// <summary>
    /// A human-readable description of the transaction.
    /// </summary>
    public string Description { get; set; } = null!;

    /// <summary>
    /// The type of transaction (TopUp, Spend, Expire, Refund).
    /// </summary>
    public TransactionType Type { get; set; }

    #region Navigation Properties

    /// <summary>
    /// The user who owns this transaction.
    /// </summary>
    public User User { get; set; } = null!;

    /// <summary>
    /// The line items of this transaction.
    /// </summary>
    public ICollection<TransactionDetail> TransactionDetails { get; set; } = [];

    #endregion
}