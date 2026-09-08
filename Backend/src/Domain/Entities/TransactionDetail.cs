using MyTarotReader.Domain.Common;

namespace MyTarotReader.Domain.Entities;

/// <summary>
/// Represents a line item within a transaction. Each detail can optionally reference a white coin batch
/// when the transaction involves white coin consumption or expiration.
/// </summary>
public class TransactionDetail : BaseEntity
{
    /// <summary>
    /// The identifier of the parent transaction.
    /// </summary>
    public Guid TransactionId { get; set; }

    /// <summary>
    /// The identifier of the white coin batch involved in this detail (nullable — only set for white coin spend/expire).
    /// </summary>
    public Guid? WhiteCoinBatchId { get; set; }

    /// <summary>
    /// The coin amount for this detail (positive for credit, negative for debit depending on transaction type).
    /// </summary>
    public int Amount { get; set; }

    #region Navigation Properties

    /// <summary>
    /// The parent transaction.
    /// </summary>
    public Transaction Transaction { get; set; } = null!;

    /// <summary>
    /// The white coin batch referenced by this detail (if any).
    /// </summary>
    public WhiteCoinBatch? WhiteCoinBatch { get; set; }

    #endregion
}