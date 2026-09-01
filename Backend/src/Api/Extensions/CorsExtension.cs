using Microsoft.Extensions.Configuration;

namespace MyTarotReader.Api.Extensions;

public static class CorsExtension
{
    /// <summary>The name of the CORS policy registered by <see cref="AddCorsPolicy"/>.</summary>
    public const string PolicyName = "FrontendCors";

    /// <summary>
    /// Adds the CORS policy that allows the frontend origin to call the API with
    /// credentials (required because the guest cookie is HttpOnly). The allowed
    /// origin is read from <c>Cors:FrontendUrl</c>.
    /// </summary>
    /// <param name="services">The service collection to extend.</param>
    /// <param name="configuration">Configuration providing Cors:FrontendUrl.</param>
    public static IServiceCollection AddCorsPolicy(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        var frontendUrl = configuration["Cors:FrontendUrl"]
            ?? throw new InvalidOperationException("Cors:FrontendUrl is not configured.");

        services.AddCors(options =>
            options.AddPolicy(PolicyName, policy =>
                policy
                    .WithOrigins(frontendUrl)
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials()));   // required because the guest cookie is HttpOnly

        return services;
    }
}
