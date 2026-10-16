using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.OpenApi.Models;

namespace MyTarotReader.Api.Extensions;

/// <summary>
/// Aggregates all API service registrations into a single entry point for <c>Program.cs</c>.
/// </summary>
public static class AllExtension
{
    /// <summary>
    /// Registers MVC, Swagger, JSON options, and all application services
    /// (database, settings, DI, Redis, CORS, JWT authentication).
    /// </summary>
    public static IServiceCollection AddAllServices(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.MapType<object>(() => new OpenApiSchema { Type = "object" });
        });

        services
            .AddControllers()
            .AddJsonOptions(opts =>
                opts.JsonSerializerOptions.Converters.Add(
                    new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
                )
            );

        services.AddDatabase(configuration);
        services.AddSettings(configuration);
        services.AddRegister();
        services.AddRedis(configuration);
        services.AddCorsPolicy(configuration);
        services.AddJwtAuthentication(configuration);

        return services;
    }
}
