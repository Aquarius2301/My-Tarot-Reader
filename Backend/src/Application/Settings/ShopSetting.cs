namespace MyTarotReader.Application.Settings;

/// <summary>
/// Configuration for the coin shop, bound from the <c>Shop</c> appsettings section.
/// </summary>
public class ShopSetting
{
    /// <summary>The purchasable red-coin packages.</summary>
    public List<ShopPackage> Packages { get; set; } = [];
}

/// <summary>A single purchasable red-coin package.</summary>
public class ShopPackage
{
    /// <summary>Unique package code used by the client when creating an order.</summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>The number of red coins granted when the package is paid.</summary>
    public int RedCoins { get; set; }

    /// <summary>The package price in Vietnamese dong (VND).</summary>
    public int PriceVnd { get; set; }
}
