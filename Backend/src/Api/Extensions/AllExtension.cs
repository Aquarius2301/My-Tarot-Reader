using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.OpenApi.Models;
using MyTarotReader.Api.Helpers;

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
    /// <param name="services">The service collection to configure.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <param name="environment">The hosting environment; used to disable dev-only controllers outside Development.</param>
    public static IServiceCollection AddAllServices(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment
    )
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.MapType<object>(() => new OpenApiSchema { Type = "object" });
        });

        services
            .AddControllers(options =>
            {
                if (!environment.IsDevelopment())
                {
                    options.Conventions.Add(new DevelopmentOnlyControllerConvention());
                }
            })
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
