namespace MyTarotReader.Application.Settings;

/// <summary>
/// Configuration for the PayOS payment gateway, bound from the <c>PayOs</c> appsettings section.
/// </summary>
/// <remarks>
/// Credentials are read from appsettings (production values live in the secret file).
/// Empty credentials fall back to the <c>PAYOS_CLIENT_ID</c>, <c>PAYOS_API_KEY</c>,
/// <c>PAYOS_CHECKSUM_KEY</c> and <c>PAYOS_PARTNER_CODE</c> environment variables.
/// </remarks>
public class PayOsSetting
{
    /// <summary>PayOS client id (secret).</summary>
    public string ClientId { get; set; } = string.Empty;

    /// <summary>PayOS api key (secret).</summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>PayOS checksum key used to sign requests and verify webhooks (secret).</summary>
    public string ChecksumKey { get; set; } = string.Empty;

    /// <summary>Optional PayOS partner code.</summary>
    public string PartnerCode { get; set; } = string.Empty;

    /// <summary>Base URL of the PayOS merchant API.</summary>
    public string BaseUrl { get; set; } = "https://api-merchant.payos.vn";

    /// <summary>Public URL of the payment webhook endpoint registered with PayOS.</summary>
    public string WebhookUrl { get; set; } = string.Empty;

    /// <summary>Frontend URL the buyer returns to after a successful payment.</summary>
    public string ReturnUrl { get; set; } = string.Empty;

    /// <summary>Frontend URL the buyer returns to after cancelling a payment.</summary>
    public string CancelUrl { get; set; } = string.Empty;

    /// <summary>Whether the required PayOS credentials are configured (placeholders do not count).</summary>
    public bool IsConfigured => IsReal(ClientId) && IsReal(ApiKey) && IsReal(ChecksumKey);

    private static bool IsReal(string value) =>
        !string.IsNullOrWhiteSpace(value) && !value.StartsWith("YOUR_", StringComparison.OrdinalIgnoreCase);
}
