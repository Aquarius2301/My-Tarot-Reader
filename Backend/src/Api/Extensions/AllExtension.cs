using System.Text.Json;
using System.Text.Json.Serialization;
using MyTarotReader.Api.Backgrounds;

namespace MyTarotReader.Api.Extensions;

/// <summary>
/// Central entry point that registers every API-layer extension (Swagger,
/// controllers, database, Redis, CORS, dependency injection) in one call.
/// </summary>
public static class AllExtension
{
    /// <summary>
    /// Applies all API extensions to the service collection.
    /// </summary>
    /// <param name="services">The service collection to extend.</param>
    /// <param name="configuration">Application configuration used by the extensions.</param>
    public static IServiceCollection AddAllExtensions(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
        services
            .AddControllers()
            .AddJsonOptions(opts =>
                opts.JsonSerializerOptions.Converters.Add(
                    new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
                )
            );
        services.AddDatabase(configuration);
        services.AddRedis(configuration);
        services.AddCorsPolicy(configuration);
        services.AddDIExtensions();
        services.AddSettings(configuration);
        services.AddJwtAuthentication(configuration);
        services.AddHostedService<TokenCleanupBackgroundService>();

        return services;
    }
}
