namespace MyTarotReader.Api.Extensions;

/// <summary>
/// Configures the CORS policy allowing the frontend origin to send credentialed requests.
/// </summary>
public static class CorsExtension
{
    /// <summary>Name of the registered CORS policy.</summary>
    public const string PolicyName = "FrontendCors";

    /// <summary>Name of the permissive CORS policy applied to the health endpoint.</summary>
    public const string HealthPolicyName = "HealthCors";

    /// <summary>
    /// Registers a CORS policy allowing credentials from the configured frontend URL,
    /// required because the guest cookie is HttpOnly.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="configuration">The application configuration, requiring "Cors:FrontendUrl".</param>
    /// <exception cref="InvalidOperationException">Thrown when "Cors:FrontendUrl" is not configured.</exception>
    public static IServiceCollection AddCorsPolicy(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        var frontendUrl =
            configuration["Cors:FrontendUrl"]
            ?? throw new InvalidOperationException("Cors:FrontendUrl is not configured.");

        services.AddCors(options =>
        {
            options.AddPolicy(
                PolicyName,
                policy =>
                    policy
                        .WithOrigins(frontendUrl)
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials()
            ); // required because the guest cookie is HttpOnly

            // Health must stay reachable from any origin (Render wake-up checks,
            // monitors, browsers) without being restricted like the API itself.
            options.AddPolicy(
                HealthPolicyName,
                policy => policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()
            );
        });

        return services;
    }
}
