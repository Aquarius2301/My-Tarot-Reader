using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MyTarotReader.Application.Contracts.Persistence;
using MyTarotReader.Application.Settings;

namespace MyTarotReader.Api.Backgrounds;

/// <summary>
/// Hosted background service that periodically purges refresh tokens the application no longer
/// needs: rows that have been revoked (soft-deleted via <see cref="BaseEntity.DeletedAt"/>) and
/// rows that expired without ever being revoked. This keeps the <c>RefreshTokens</c> table from
/// growing unbounded, since every login and token rotation inserts a new row.
/// </summary>
public sealed class TokenCleanupBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly TokenCleanupSetting _settings;
    private readonly ILogger<TokenCleanupBackgroundService> _logger;

    public TokenCleanupBackgroundService(
        IServiceScopeFactory scopeFactory,
        IOptions<TokenCleanupSetting> settings,
        ILogger<TokenCleanupBackgroundService> logger
    )
    {
        _scopeFactory = scopeFactory;
        _settings = settings.Value;
        _logger = logger;
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var interval = TimeSpan.FromMinutes(_settings.IntervalMinutes);

        // Delay first so we don't hammer the database on startup; the delay is cancellable.
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(interval, stoppingToken);

                var deleted = await DeleteStaleTokensAsync(stoppingToken);
                _logger.LogInformation("Cleaned up {Count} refresh tokens", deleted);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                // Host is shutting down; exit cleanly.
                break;
            }
            catch (Exception ex)
            {
                // A transient database error must not kill the host; log and retry next interval.
                _logger.LogError(ex, "Refresh token cleanup failed; will retry on next interval");
            }
        }
    }

    private async Task<int> DeleteStaleTokensAsync(CancellationToken cancellationToken)
    {
        // The DbContext is registered as scoped, so resolve it from a new scope per run.
        await using var scope = _scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<IAppDbContext>();

        var now = DateTimeOffset.UtcNow;
        return await db
            .RefreshTokens.Where(r =>
                r.DeletedAt != null || (r.DeletedAt == null && r.ExpiresAt < now)
            )
            .ExecuteDeleteAsync(cancellationToken);
    }
}
