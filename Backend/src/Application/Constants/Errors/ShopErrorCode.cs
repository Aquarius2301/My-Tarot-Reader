namespace MyTarotReader.Application.Constants.Errors;

public class ShopErrorCode
{
    private const string Prefix = "error.shop.";

    public const string InvalidPackage = $"{Prefix}invalidPackage";
    public const string OrderNotFound = $"{Prefix}orderNotFound";
    public const string CreatePaymentFailed = $"{Prefix}createPaymentFailed";
    public const string InvalidWebhookSignature = $"{Prefix}invalidWebhookSignature";
    public const string ConfirmWebhookFailed = $"{Prefix}confirmWebhookFailed";
    public const string PayOsError = $"{Prefix}payOsError";
}
