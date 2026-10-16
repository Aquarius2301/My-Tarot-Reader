using MyTarotReader.Domain.Common;

namespace MyTarotReader.Domain.Entities;

/// <summary>
/// Admin-managed purchasable coin package offered in the shop.
/// </summary>
public class ShopPackage : BaseEntity
{
    /// <summary> Unique package code used by the client when creating an order. </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary> The number of red coins granted when the package is paid. </summary>
    public int RedCoins { get; set; }

    /// <summary> The number of bonus white coins granted when the package is paid. </summary>
    public int WhiteCoins { get; set; }

    /// <summary> The original (pre-discount) package price in Vietnamese dong (VND). </summary>
    public int OldPrice { get; set; }

    /// <summary> The current package price in Vietnamese dong (VND). </summary>
    public int Price { get; set; }

    /// <summary> Whether this package is offered to users. </summary>
    public bool IsEnabled { get; set; } = true;
}
