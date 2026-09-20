namespace MyTarotReader.Application.Contracts.Backgrounds;

/// <summary>
/// A welcome email message awaiting background delivery.
/// </summary>
/// <param name="ToEmail">The recipient email address.</param>
/// <param name="ToName">The recipient display name.</param>
/// <param name="Locale">The recipient language preference; falls back to Vietnamese when <c>null</c>.</param>
public record WelcomeEmailMessage(string ToEmail, string ToName, string? Locale);

/// <summary>
/// Queues email messages that are delivered asynchronously by a background worker,
/// outside of the HTTP request lifetime.
/// </summary>
public interface IEmailBackgroundQueue
{
    /// <summary>
    /// Queues a welcome email message for background delivery.
    /// </summary>
    /// <param name="message">The welcome email message to queue.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    ValueTask EnqueueAsync(
        WelcomeEmailMessage message,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Waits for and retrieves the next queued welcome email message.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The next queued message.</returns>
    ValueTask<WelcomeEmailMessage> DequeueAsync(CancellationToken cancellationToken = default);
}