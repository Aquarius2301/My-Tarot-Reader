namespace MyTarotReader.Application.Settings;

/// <summary>
/// Configuration for JWT issuing and validation.
/// Bound from the <c>Jwt</c> appsettings section.
/// </summary>
public class JwtSetting
{
    /// <summary>Symmetric key used to sign and validate JWT tokens.</summary>
    public string SecretKey { get; set; } = string.Empty;

    /// <summary>Expected issuer ("iss") claim value.</summary>
    public string Issuer { get; set; } = string.Empty;

    /// <summary>Expected audience ("aud") claim value.</summary>
    public string Audience { get; set; } = string.Empty;

    /// <summary>Lifetime of an access token in minutes.</summary>
    public int AccessTokenDurationMinutes { get; set; } = 480;

    /// <summary>Lifetime of a refresh token in days.</summary>
    public int RefreshTokenDurationDays { get; set; } = 7;
}
