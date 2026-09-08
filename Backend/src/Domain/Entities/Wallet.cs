using MyTarotReader.Domain.Common;

namespace MyTarotReader.Domain.Entities;

/// <summary>
/// Represents the Wallets table in the database.
/// Stores the coin balances for a user.
/// </summary>
public class Wallet : BaseEntity
{
    /// <summary>
    /// The identifier of the owning user.
    /// </summary>
    public Guid UserId { get; set; }

    public int WhiteCoin =>
        WhiteCoinBatches
            .Where(x => x.ExpiredAt >= DateTimeOffset.UtcNow)
            .Sum(x => x.RemainingAmount);

    /// <summary>
    /// The user's red coin balance (no expiration date).
    /// </summary>
    public int RedCoin { get; set; } = 0;

    public DateTimeOffset UpdatedAt { get; set; }

    #region Navigation Properties

    /// <summary>
    /// The owning user.
    /// </summary>
    public User User { get; set; } = null!;

    public List<WhiteCoinBatch> WhiteCoinBatches { get; set; } = [];
    #endregion
}
