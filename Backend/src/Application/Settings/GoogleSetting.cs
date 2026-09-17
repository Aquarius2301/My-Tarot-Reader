namespace MyTarotReader.Application.Settings;

/// <summary>
/// Configuration for validating Google OAuth credentials.
/// Bound from the <c>Google</c> appsettings section.
/// </summary>
public class GoogleSetting
{
    /// <summary>OAuth client ID issued by Google, used to validate the credential audience.</summary>
    public string ClientId { get; set; } = string.Empty;
}
