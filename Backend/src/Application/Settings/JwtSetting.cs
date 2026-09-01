using System;

namespace MyTarotReader.Application.Settings;

public class JwtSetting
{
    public string SecretKey { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;

    /// <summary>Lifetime of an access token in minutes.</summary>
    public int AccessTokenDurationMinutes { get; set; } = 480;

    /// <summary>Lifetime of a refresh token in days.</summary>
    public int RefreshTokenDurationDays { get; set; } = 7;
}
