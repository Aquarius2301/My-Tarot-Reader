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

    /// <summary>
    /// The user's white coin balance.
    /// </summary>
    public int WhiteCoin { get; set; } = 0;

    /// <summary>
    /// The user's red coin balance.
    /// </summary>
    public int RedCoin { get; set; } = 0;

    #region Navigation Properties

    /// <summary>
    /// The owning user.
    /// </summary>
    public User User { get; set; } = null!;
    #endregion
}
