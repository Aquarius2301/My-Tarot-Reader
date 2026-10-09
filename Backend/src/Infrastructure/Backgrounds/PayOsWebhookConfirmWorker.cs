using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MyTarotReader.Application.Contracts.Common;
using MyTarotReader.Application.Settings;

namespace MyTarotReader.Infrastructure.Backgrounds;

/// <summary>
/// Registers the payment webhook URL with PayOS once at startup so payment
/// notifications are delivered to <c>api/shop/webhook/payos</c>.
/// </summary>
public class PayOsWebhookConfirmWorker(
    IServiceScopeFactory scopeFactory,
    IOptions<PayOsSetting> setting,
    ILogger<PayOsWebhookConfirmWorker> logger
) : BackgroundService
{
    /// <inheritdoc />
    /// <remarks>
    /// Confirmation is attempted once; failures (e.g. the URL is unreachable from PayOS
    /// during local development) are logged and do not prevent the app from starting.
    /// </remarks>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var webhookUrl = setting.Value.WebhookUrl;
        if (string.IsNullOrWhiteSpace(webhookUrl))
        {
            logger.LogWarning(
                "PayOs:WebhookUrl is not configured; payment notifications will not be received."
            );
            return;
        }

        try
        {
            // Wait until the app is listening so PayOS can reach the validation ping.
            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
        }
        catch (OperationCanceledException)
        {
            return;
        }

        using var scope = scopeFactory.CreateScope();
        var payOsClient = scope.ServiceProvider.GetRequiredService<IPayOsClient>();

        try
        {
            await payOsClient.ConfirmWebhookAsync(webhookUrl, stoppingToken);
            logger.LogInformation("Registered PayOS webhook URL {WebhookUrl}.", webhookUrl);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // Shutting down.
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to register PayOS webhook URL {WebhookUrl}.", webhookUrl);
        }
    }
}
