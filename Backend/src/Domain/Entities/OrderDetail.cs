using MyTarotReader.Domain.Common;

namespace MyTarotReader.Domain.Entities;

public class OrderDetail : BaseEntity
{
    public Guid OrderId { get; set; }

    public Guid? WhiteCoinBatchId { get; set; }

    public int Amount { get; set; }

    #region Navigation Properties

    public Order Order { get; set; } = null!;

    public WhiteCoinBatch? WhiteCoinBatch { get; set; }

    #endregion
}
