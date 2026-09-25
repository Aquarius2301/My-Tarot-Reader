namespace MyTarotReader.Application.Constants.Errors;

public class WalletErrorCode
{
    private const string Prefix = "error.wallet.";

    public const string InvalidAmount = $"{Prefix}invalidAmount";
    public const string WalletNotFound = $"{Prefix}walletNotFound";
    public const string InsufficientCoins = $"{Prefix}insufficientCoins";
    public const string InsufficientRedCoin = $"{Prefix}insufficientRedCoin";
}