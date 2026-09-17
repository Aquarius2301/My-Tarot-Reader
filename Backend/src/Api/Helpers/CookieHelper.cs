namespace MyTarotReader.Api.Helpers;

/// <summary>
/// Centralizes creation, appending, and deletion of authentication-related cookies
/// with consistent security options.
/// </summary>
public static class CookieHelper
{
    /// <summary>Cookie name for the access token.</summary>
    public const string AccessTokenCookieName = "accessToken";

    /// <summary>Cookie name for the refresh token.</summary>
    public const string RefreshTokenCookieName = "refreshToken";

    /// <summary>Cookie name for marking a guest session.</summary>
    public const string GuestCookieName = "guest";

    /// <summary>
    /// Builds the standard <see cref="CookieOptions"/> used for all auth cookies.
    /// </summary>
    /// <param name="expires">Expiration time of the cookie.</param>
    public static CookieOptions BuildOptions(DateTimeOffset expires) =>
        new()
        {
            HttpOnly = true,
            IsEssential = true,
            SameSite = SameSiteMode.None,
            Secure = true,
            Path = "/",
            Expires = expires,
        };

    /// <summary>
    /// Appends a cookie to the response using the standard auth cookie options.
    /// </summary>
    /// <param name="response">The response to append the cookie to.</param>
    /// <param name="key">Cookie name.</param>
    /// <param name="value">Cookie value.</param>
    /// <param name="expires">Expiration time of the cookie.</param>
    public static void Append(
        HttpResponse response,
        string key,
        string value,
        DateTimeOffset expires
    ) => response.Cookies.Append(key, value, BuildOptions(expires));

    /// <summary>
    /// Deletes a cookie from the client, matching the options it was set with.
    /// </summary>
    /// <param name="response">The response to remove the cookie from.</param>
    /// <param name="key">Cookie name to delete.</param>
    public static void Delete(HttpResponse response, string key) =>
        response.Cookies.Delete(
            key,
            new CookieOptions
            {
                Path = "/",
                HttpOnly = true,
                SameSite = SameSiteMode.None,
                Secure = true,
            }
        );
}
