using MyTarotReader.Application.Settings;

namespace MyTarotReader.Api.Extensions;

/// <summary>
/// Binds strongly-typed settings classes to their corresponding configuration sections.
/// </summary>
public static class SettingExtension
{
    /// <summary>
    /// Registers <see cref="IOptions{TOptions}"/> bindings for Jwt, Google, AiTarot,
    /// TokenCleanup, Wallet, and Email settings.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="configuration">The application configuration.</param>
    public static void AddSettings(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtSetting>(configuration.GetSection("Jwt"));
        services.Configure<GoogleSetting>(configuration.GetSection("Google"));
        services.Configure<AiTarotSetting>(configuration.GetSection("AiTarot"));
        services.Configure<TokenCleanupSetting>(configuration.GetSection("TokenCleanup"));
        services.Configure<WalletSetting>(configuration.GetSection("Wallet"));
        services.Configure<EmailSetting>(configuration.GetSection("Email"));
    }
}
