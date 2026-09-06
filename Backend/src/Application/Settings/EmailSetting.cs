namespace MyTarotReader.Application.Settings;

/// <summary>
/// Configuration for the email sending service via SMTP.
/// Bound from the <c>Email</c> appsettings section.
/// </summary>
public class EmailSetting
{
    /// <summary>
    /// SMTP server host name or IP address.
    /// </summary>
    public string Host { get; set; } = string.Empty;

    /// <summary>
    /// SMTP server port (e.g. 587 for STARTTLS, 465 for implicit TLS).
    /// </summary>
    public int Port { get; set; } = 587;

    /// <summary>
    /// Username used to authenticate with the SMTP server.
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Password (or app password) used to authenticate with the SMTP server.
    /// </summary>
    public string Password { get; set; } = string.Empty;

    /// <summary>
    /// Email address shown as the sender of outgoing messages.
    /// For Gmail SMTP this must be the account's own Gmail address.
    /// </summary>
    public string FromAddress { get; set; } = string.Empty;

    /// <summary>
    /// Display name shown alongside the sender email address.
    /// </summary>
    public string FromName { get; set; } = string.Empty;

    /// <summary>
    /// Whether to use TLS/SSL when connecting to the SMTP server.
    /// </summary>
    public bool EnableSsl { get; set; } = true;
}