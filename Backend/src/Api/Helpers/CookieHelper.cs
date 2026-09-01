using Microsoft.AspNetCore.Http;

namespace MyTarotReader.Api.Helpers;

/// <summary>
/// Single source of truth for cookie names and the shared secure cookie options used across the API.
/// All auth/guest cookies are HttpOnly + SameSite=None + Secure so they round-trip with credentialed CORS.
/// </summary>
public static class CookieHelper
{
    public const string AccessTokenCookieName = "accessToken";
    public const string RefreshTokenCookieName = "refreshToken";
    public const string GuestCookieName = "guest";

    /// <summary>Builds the shared HttpOnly cookie options with an absolute expiry.</summary>
    /// <param name="expires">The absolute UTC expiry for the cookie.</param>
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

    /// <summary>Appends a cookie using the shared secure options.</summary>
    /// <param name="response">The HTTP response to write the cookie to.</param>
    /// <param name="key">The cookie name.</param>
    /// <param name="value">The cookie value.</param>
    /// <param name="expires">The absolute UTC expiry for the cookie.</param>
    public static void Append(HttpResponse response, string key, string value, DateTimeOffset expires) =>
        response.Cookies.Append(key, value, BuildOptions(expires));

    /// <summary>Deletes a cookie using the shared secure options.</summary>
    /// <param name="response">The HTTP response to clear the cookie on.</param>
    /// <param name="key">The cookie name.</param>
    public static void Delete(HttpResponse response, string key) =>
        response.Cookies.Delete(key, new CookieOptions { Path = "/", SameSite = SameSiteMode.None, Secure = true });
}
