namespace MyTarotReader.Application.Settings;

/// <summary>
/// Configuration for the email sending service via the Resend HTTPS API.
/// Bound from the <c>Email</c> appsettings section.
/// </summary>
public class EmailSetting
{
    /// <summary>Resend API key (secret) used to authenticate the request.</summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>Base URL of the Resend API.</summary>
    public string Endpoint { get; set; } = "https://api.resend.com";

    /// <summary>
    /// Email address shown as the sender of outgoing messages.
    /// The domain must be verified in the Resend dashboard.
    /// </summary>
    public string FromAddress { get; set; } = string.Empty;

    /// <summary>Display name shown alongside the sender email address.</summary>
    public string FromName { get; set; } = string.Empty;
}
