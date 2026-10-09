namespace MyTarotReader.Domain.Enums;

/// <summary> The status of a red-coin shop payment order. </summary>
public enum PaymentOrderStatus
{
    /// <summary> The payment link was created and is awaiting payment. </summary>
    Pending,

    /// <summary> Payment succeeded and the red coins have been credited. </summary>
    Paid,

    /// <summary> The payment was cancelled before completion. </summary>
    Cancelled,

    /// <summary> The payment link expired before completion. </summary>
    Expired,

    /// <summary> Creating the payment link or the payment itself failed. </summary>
    Failed,
}
