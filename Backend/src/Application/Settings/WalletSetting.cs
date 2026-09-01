namespace MyTarotReader.Application.Settings;

/// <summary>Configuration for wallet-related settings.</summary>
public class WalletSetting
{
    /// <summary>
    /// Initial number of white coins granted to a new user upon registration.
    /// Bound from the <c>Wallet:InitialWhiteCoins</c> appsettings section.
    /// </summary>
    public int InitialWhiteCoins { get; set; } = 5;
}