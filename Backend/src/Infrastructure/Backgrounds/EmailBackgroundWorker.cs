using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MyTarotReader.Application.Contracts.Backgrounds;
using MyTarotReader.Application.Contracts.Common;

namespace MyTarotReader.Infrastructure.Backgrounds;

/// <summary>
/// A hosted background worker that delivers queued welcome emails in a dedicated
/// service scope, independent of the HTTP request that enqueued them.
/// </summary>
public class EmailBackgroundWorker(
    IServiceScopeFactory scopeFactory,
    IEmailBackgroundQueue queue,
    ILogger<EmailBackgroundWorker> logger
) : BackgroundService
{
    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            WelcomeEmailMessage message;
            try
            {
                message = await queue.DequeueAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }

            using var scope = scopeFactory.CreateScope();
            var emailHandler = scope.ServiceProvider.GetRequiredService<IEmailHandler>();

            try
            {
                await emailHandler.SendWelcomeEmailAsync(
                    message.ToEmail,
                    message.ToName,
                    message.Locale,
                    stoppingToken
                );
            }
            catch (OperationCanceledException)
            {
                // Application is shutting down; stop processing.
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to send welcome email to {Email}", message.ToEmail);
            }
        }
    }
}