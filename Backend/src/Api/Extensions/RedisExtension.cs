using StackExchange.Redis;

namespace MyTarotReader.Api.Extensions;

/// <summary>
/// Configures the Redis connection multiplexer.
/// </summary>
public static class RedisExtension
{
    /// <summary>
    /// Registers a singleton <see cref="IConnectionMultiplexer"/> configured to start
    /// even if Redis is briefly unavailable, retrying the connection in the background.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="configuration">The application configuration, requiring "Redis:Configuration".</param>
    /// <exception cref="InvalidOperationException">Thrown when "Redis:Configuration" is not configured.</exception>
    public static IServiceCollection AddRedis(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        var configured = configuration["Redis:Configuration"];

        if (string.IsNullOrWhiteSpace(configured))
        {
            throw new InvalidOperationException("Redis:Configuration is not configured.");
        }

        // AbortOnConnectFail=false lets the app start even if Redis is briefly
        // unavailable; connects are attempted lazily and retried in the background.
        var options = BuildRedisOptions(configured);
        options.AbortOnConnectFail = false;
        options.ConnectTimeout = 5000;

        services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(options));

        return services;
    }

    /// <summary>
    /// Parses a Redis connection string, supporting both a "host:port" style string
    /// and a "redis(s)://user:password@host:port" URI (e.g. Upstash).
    /// </summary>
    /// <param name="connectionString">The raw connection string from configuration.</param>
    private static ConfigurationOptions BuildRedisOptions(string connectionString)
    {
        if (
            !Uri.TryCreate(connectionString, UriKind.Absolute, out var uri)
            || (uri.Scheme is not "rediss" and not "redis")
        )
        {
            // Localhost-style "host:port" (or already-semicolon config).
            return ConfigurationOptions.Parse(connectionString);
        }

        var options = new ConfigurationOptions
        {
            EndPoints = { { uri.Host, uri.Port } },
            Ssl = uri.Scheme.Equals("rediss", StringComparison.OrdinalIgnoreCase),
            // Upstash requires both user and password on the connection.
            User = Uri.UnescapeDataString(
                uri.UserInfo.Contains(':') ? uri.UserInfo.Split(':')[0] : uri.UserInfo
            ),
            Password = Uri.UnescapeDataString(
                uri.UserInfo.Contains(':') ? uri.UserInfo.Split(':')[1] : string.Empty
            ),
        };

        return options;
    }
}
