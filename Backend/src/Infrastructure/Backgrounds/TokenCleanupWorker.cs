using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MyTarotReader.Application.Contracts.Persistence;
using MyTarotReader.Application.Settings;

namespace MyTarotReader.Infrastructure.Backgrounds;

/// <summary>
/// A hosted background worker that periodically physically deletes refresh tokens
/// that are either expired or soft-deleted, keeping the token table from growing unbounded.
/// </summary>
public class TokenCleanupWorker(
    IServiceScopeFactory scopeFactory,
    IOptions<TokenCleanupSetting> setting,
    ILogger<TokenCleanupWorker> logger
) : BackgroundService
{
    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(setting.Value.IntervalMinutes));

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CleanupOnceAsync(DateTimeOffset.UtcNow, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to clean up expired or revoked refresh tokens");
            }

            try
            {
                await timer.WaitForNextTickAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    /// <summary>
    /// Runs a single cleanup pass: physically deletes refresh tokens that have
    /// expired or been soft-deleted, resolving the DB context in its own scope.
    /// </summary>
    /// <param name="now">The reference time used to decide token expiry.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The number of refresh tokens deleted.</returns>
    internal async Task<int> CleanupOnceAsync(
        DateTimeOffset now,
        CancellationToken cancellationToken = default
    )
    {
        using var scope = scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<IAppDbContext>();

        var staleTokens = await context
            .RefreshTokens.IgnoreQueryFilters()
            .Where(rt => rt.ExpiresAt < now || rt.DeletedAt != null)
            .ToListAsync(cancellationToken);

        if (staleTokens.Count == 0)
        {
            return 0;
        }

        context.RefreshTokens.RemoveRange(staleTokens);
        var deleted = await context.SaveChangesAsync(cancellationToken);

        if (deleted > 0)
        {
            logger.LogInformation("Deleted {Count} expired or revoked refresh tokens", deleted);
        }

        return deleted;
    }
}