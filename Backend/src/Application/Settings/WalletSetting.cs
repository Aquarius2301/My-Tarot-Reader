namespace MyTarotReader.Application.Settings;

/// <summary>
/// Configuration for wallet-related settings.
/// Bound from the <c>Wallet</c> appsettings section.
/// </summary>
public class WalletSetting
{
    /// <summary>Initial number of white coins granted to a new user upon registration.</summary>
    public int InitialWhiteCoins { get; set; } = 5;

    /// <summary>
    /// Number of days after which a granted white coin batch expires and becomes unusable.
    /// </summary>
    public int ExpireDays { get; set; } = 30;
}
