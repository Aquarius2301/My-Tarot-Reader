using MyTarotReader.Application.Exceptions;

namespace MyTarotReader.Application.Contracts.Services;

/// <summary>
/// Sends emails on behalf of the application (welcome, notifications, ...).
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Sends a welcome email to a newly signed-up user.
    /// </summary>
    /// <param name="toEmail">The recipient's email address.</param>
    /// <param name="toName">The recipient's display name used in the greeting.</param>
    /// <param name="language">The language of the welcome email ("vi" or "en"); defaults to Vietnamese when null or unknown.</param>
    /// <param name="cancellationToken">Token to observe for task cancellation.</param>
    /// <returns>A task that completes when the email has been handed to the SMTP server.</returns>
    /// <exception cref="BadRequestException">Thrown when <paramref name="toEmail"/> is malformed.</exception>
    /// <exception cref="InternalServerException">Thrown when the email could not be sent.</exception>
    Task SendWelcomeEmailAsync(
        string toEmail,
        string toName,
        string? language,
        CancellationToken cancellationToken = default);
}
