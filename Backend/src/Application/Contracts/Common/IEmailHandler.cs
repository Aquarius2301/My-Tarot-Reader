namespace MyTarotReader.Application.Contracts.Common;

/// <summary>
/// Composes and sends predefined transactional emails for application events.
/// </summary>
public interface IEmailHandler
{
    /// <summary>
    /// Sends a welcome email to the specified recipient with the given name and language preference.
    /// </summary>
    /// <param name="toEmail">The email address of the recipient.</param>
    /// <param name="toName">The name of the recipient.</param>
    /// <param name="language">The language preference of the recipient. Falls back to a default if <c>null</c>.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task SendWelcomeEmailAsync(
        string toEmail,
        string toName,
        string? language,
        CancellationToken cancellationToken = default
    );
}
