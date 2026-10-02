namespace MyTarotReader.Application.Settings;

/// <summary>
/// Configuration for the development-only fake login used to obtain a JWT
/// without going through Google OAuth, so JWT-protected endpoints can be
/// exercised from Swagger UI.
/// </summary>
/// <remarks>
/// The endpoint backing these settings is marked <c>[DevelopmentOnly]</c> and is
/// removed from the application model outside the Development environment.
/// </remarks>
public class DevAuthSetting
{
    /// <summary>Provider key of the seeded development user; acts as its unique identity.</summary>
    public string ProviderKey { get; set; } = "dev-swagger";

    /// <summary>Email of the seeded development user.</summary>
    public string Email { get; set; } = "dev@my-tarot-reader.local";

    /// <summary>Display name of the seeded development user.</summary>
    public string FullName { get; set; } = "Swagger Dev User";

    /// <summary>
    /// White coins granted to the development user the first time it is seeded,
    /// so coin-deducting endpoints stay testable without topping up manually.
    /// </summary>
    public int InitialWhiteCoins { get; set; } = 1000;

    /// <summary>
    /// Number of days the seeded white coin batch stays valid.
    /// </summary>
    public int WhiteCoinExpireDays { get; set; } = 365;

    /// <summary>
    /// Device fingerprint used when the request omits the "X-Device-Id" header,
    /// so the issued refresh token is still bound to a stable value.
    /// </summary>
    public string DefaultDeviceFingerprint { get; set; } = "swagger-dev";
}
