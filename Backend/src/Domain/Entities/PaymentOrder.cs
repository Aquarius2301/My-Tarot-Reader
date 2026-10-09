using MyTarotReader.Domain.Common;
using MyTarotReader.Domain.Enums;

namespace MyTarotReader.Domain.Entities;

/// <summary> A red-coin purchase order paid through PayOS. </summary>
public class PaymentOrder : BaseEntity
{
    public Guid UserId { get; set; }

    /// <summary> Unique PayOS order code (also used as the lookup key for webhooks). </summary>
    public long OrderCode { get; set; }

    /// <summary> The shop package code purchased by the user. </summary>
    public string PackageCode { get; set; } = null!;

    /// <summary> The order price in Vietnamese dong (VND). </summary>
    public int AmountVnd { get; set; }

    /// <summary> The number of red coins granted when the order is paid. </summary>
    public int RedCoins { get; set; }

    public PaymentOrderStatus Status { get; set; }

    /// <summary> The PayOS payment link id, set after the payment link is created. </summary>
    public string? PayOsPaymentLinkId { get; set; }

    /// <summary> The bank transaction reference reported by PayOS when the order was paid. </summary>
    public string? PayOsTransactionReference { get; set; }

    /// <summary> The time the payment was confirmed and the coins were credited. </summary>
    public DateTimeOffset? PaidAt { get; set; }

    #region Navigation Properties

    public User User { get; set; } = null!;

    #endregion
}
