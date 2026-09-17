using MyTarotReader.Domain.Common;

namespace MyTarotReader.Domain.Entities;

public class Wallet : BaseEntity
{
    public Guid UserId { get; set; }

    /// <summary>
    /// The user's paid coin balance. White coin balance is instead derived from
    /// the sum of active, non-expired <see cref="WhiteCoinBatches"/>.
    /// </summary>
    public int RedCoin { get; set; } = 0;

    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    #region Navigation Properties

    public User User { get; set; } = null!;

    public List<WhiteCoinBatch> WhiteCoinBatches { get; set; } = [];
    #endregion
}
