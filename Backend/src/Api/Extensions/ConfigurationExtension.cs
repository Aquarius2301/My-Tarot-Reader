using Microsoft.Extensions.Configuration;

namespace MyTarotReader.Api.Extensions;

/// <summary>
/// Loads a whole settings file as a configuration source, so production secrets can be
/// maintained as one JSON document instead of a set of <c>A__B</c> environment variables.
/// </summary>
public static class ConfigurationExtension
{
    /// <summary>Path Render mounts secret files to.</summary>
    public const string DefaultSecretFilePath = "/etc/secrets/appsettings.Production.json";

    /// <summary>Environment variable that overrides <see cref="DefaultSecretFilePath"/>.</summary>
    public const string SecretFilePathVariable = "SETTINGS_FILE";

    /// <summary>
    /// Resolves the settings file path to load, or <c>null</c> when the current
    /// environment does not use a settings file.
    /// </summary>
    /// <param name="configuration">The application configuration, used to read the path override.</param>
    /// <param name="environment">The host environment; only production loads a settings file.</param>
    public static string? GetSecretFilePath(
        this IConfiguration configuration,
        IHostEnvironment environment
    )
    {
        if (!environment.IsProduction())
            return null;

        var configured = configuration[SecretFilePathVariable];

        return string.IsNullOrWhiteSpace(configured) ? DefaultSecretFilePath : configured;
    }

    /// <summary>
    /// Adds the settings file as the highest-priority configuration source.
    /// </summary>
    /// <remarks>
    /// Must be called before the configuration is consumed (before the service
    /// registrations that read settings eagerly), otherwise those values are already
    /// captured without them.
    /// </remarks>
    /// <param name="configuration">The configuration builder.</param>
    /// <param name="path">The settings file path.</param>
    /// <returns><c>true</c> when the file exists and was added; otherwise <c>false</c>.</returns>
    public static bool AddSecretFileIfExists(this IConfigurationBuilder configuration, string path)
    {
        if (!File.Exists(path))
            return false;

        configuration.AddJsonFile(path, optional: false, reloadOnChange: false);

        return true;
    }
}
