using MyTarotReader.Application.Settings;

namespace MyTarotReader.Api.Extensions;

/// <summary>
/// Extension methods for configuring application settings.
/// </summary>
public static class SettingExtension
{
    /// <summary>
    /// Adds settings configuration from appsettings.json to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration.</param>
    public static void AddSettings(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtSetting>(configuration.GetSection("Jwt"));
        services.Configure<GoogleSetting>(configuration.GetSection("Google"));
        services.Configure<AiTarotSetting>(configuration.GetSection("AiTarot"));
        services.Configure<TokenCleanupSetting>(configuration.GetSection("TokenCleanup"));
        services.Configure<WalletSetting>(configuration.GetSection("Wallet"));
    }
}
