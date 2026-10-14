using MyTarotReader.Domain.Common;
using MyTarotReader.Domain.Enums;

namespace MyTarotReader.Domain.Entities;

public class Order : BaseEntity
{
    public Guid UserId { get; set; }

    public int RedCoins { get; set; }

    public int WhiteCoins { get; set; }

    public string Description { get; set; } = null!;

    public OrderType Type { get; set; }

    #region Navigation Properties

    public User User { get; set; } = null!;

    // public ICollection<WhiteCoinBatch> WhiteCoinsBatches { get; set; } = [];

    #endregion
}
