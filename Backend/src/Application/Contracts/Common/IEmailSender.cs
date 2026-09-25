namespace MyTarotReader.Application.Contracts.Common;

/// <summary>
/// Sends raw emails through the underlying mail transport (Resend HTTPS API).
/// </summary>
public interface IEmailSender
{
    /// <summary>
    /// Sends an email to the specified recipient with the given subject and HTML body.
    /// </summary>
    /// <param name="toEmail">The email address of the recipient.</param>
    /// <param name="toName">
    /// The name of the recipient. The Resend API only accepts plain recipient
    /// addresses, so the value is not forwarded to the transport.
    /// </param>
    /// <param name="subject">The subject of the email.</param>
    /// <param name="htmlBody">The HTML content of the email body.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task SendAsync(
        string toEmail,
        string toName,
        string subject,
        string htmlBody,
        CancellationToken cancellationToken = default
    );
}
