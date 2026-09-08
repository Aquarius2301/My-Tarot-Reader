namespace MyTarotReader.Domain.Enums;

/// <summary>
/// The type of a coin transaction.
/// </summary>
public enum TransactionType
{
    /// <summary>
    /// User purchased coins (added to balance).
    /// </summary>
    TopUp,

    /// <summary>
    /// User spent coins on a service (deducted from balance).
    /// </summary>
    Spend,

    /// <summary>
    /// White coins expired and were removed from balance.
    /// </summary>
    Expire,

    /// <summary>
    /// Coins refunded back to user balance.
    /// </summary>
    Refund,
}