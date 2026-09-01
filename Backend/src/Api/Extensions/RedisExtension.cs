using System;
using StackExchange.Redis;

namespace MyTarotReader.Api.Extensions;

public static class RedisExtension
{
    /// <summary>
    /// Registers a singleton <see cref="IConnectionMultiplexer"/> for Redis,
    /// parsing the <c>Redis:Configuration</c> connection string (plain
    /// <c>host:port</c> or a <c>redis(s)://</c> URL). Connection failures are
    /// non-fatal at startup so the app can boot before Redis is reachable.
    /// </summary>
    /// <param name="services">The service collection to extend.</param>
    /// <param name="configuration">Configuration providing Redis:Configuration.</param>
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
    /// Builds Redis <see cref="ConfigurationOptions"/> from a connection string. Upstash
    /// supplies a <c>rediss://user:password@host:port</c> URL; StackExchange.Redis's
    /// <see cref="ConfigurationOptions.Parse(string)"/> does not reliably split the URI
    /// scheme/host/credentials, so we parse it explicitly and set TLS when the scheme is
    /// <c>rediss</c>. Plain <c>host:port</c> strings pass through unaffected.
    /// </summary>
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
