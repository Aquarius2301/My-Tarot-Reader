using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyTarotReader.Api.Helpers;
using MyTarotReader.Application.Common.Exceptions;
using MyTarotReader.Application.Common.Models;
using MyTarotReader.Application.Constants.Errors;
using MyTarotReader.Application.Contracts.Services;

namespace MyTarotReader.Api.Controllers;

[Route("api/auth")]
[ApiController]
[ProducesErrorResponseType(typeof(ApiResponse<object>))]
public class AuthController(IAuthService service) : ControllerBase
{
    /// <summary>
    /// Google OAuth login and register.
    /// </summary>
    /// <remarks>
    /// Requires the "X-Device-Id" header to be set for device fingerprinting.
    /// Access and refresh tokens are set in HttpOnly cookies upon successful login.
    /// So, the client does not need to handle the tokens directly.
    /// </remarks>
    [HttpPost("oauth")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GoogleLoginAsync(
        [FromBody] GoogleLoginRequest request,
        CancellationToken cancellationToken
    )
    {
        var deviceId = Request.Headers["X-Device-Id"].ToString();
        var response = await service.GoogleLoginAsync(request, deviceId, cancellationToken);

        AppendAuthCookies(
            response.AccessToken,
            response.RefreshToken,
            response.AccessTokenMinutes,
            response.RefreshTokenDays
        );

        return Ok(ApiResponse.Success());
    }

    /// <summary>
    /// Refreshes the access token using the provided refresh token.
    /// </summary>
    /// <remarks>
    /// Requires the "X-Device-Id" header to be set for device fingerprinting.
    /// Access and refresh tokens are set in HttpOnly cookies upon successful refresh.
    /// </remarks>
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RefreshAsync(CancellationToken cancellationToken)
    {
        var refreshToken = ReadRefreshTokenOrThrow();
        var deviceId = Request.Headers["X-Device-Id"].ToString();

        var response = await service.RefreshAsync(refreshToken, deviceId, cancellationToken);

        AppendAuthCookies(
            response.AccessToken,
            response.RefreshToken,
            response.AccessTokenMinutes,
            response.RefreshTokenDays
        );

        return Ok(ApiResponse.Success());
    }

    /// <summary>
    /// Logs out the user by invalidating the provided refresh token and clearing authentication cookies.
    /// </summary>
    /// <remarks>
    /// Clears the access and refresh token cookies from the response, effectively logging out the user.
    /// The api always returns a success response, even if the refresh token is invalid or missing.
    /// </remarks>
    [HttpPost("logout")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> LogoutAsync(CancellationToken cancellationToken)
    {
        var refreshToken = Request.Cookies.TryGetValue(
            CookieHelper.RefreshTokenCookieName,
            out var token
        )
            ? token
            : string.Empty;

        await service.LogoutAsync(refreshToken, cancellationToken);

        ClearAuthCookies();

        return Ok(ApiResponse.Success());
    }

    /// <summary>
    /// Retrieves the current user's information based on the access token provided in the request.
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<GetCurrentUserResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCurrentUserAsync(CancellationToken cancellationToken)
    {
        var userId = JwtHelper.GetUserId(HttpContext);

        var user = await service.GetCurrentUserAsync(userId, cancellationToken);

        return Ok(ApiResponse.Success(user));
    }

    #region Private Helpers
    /// <summary>
    /// Reads the refresh token from its cookie.
    /// </summary>
    /// <exception cref="UnauthorizedException">
    /// Thrown when the refresh token cookie is missing or empty.
    /// </exception>
    private string ReadRefreshTokenOrThrow()
    {
        if (
            !Request.Cookies.TryGetValue(CookieHelper.RefreshTokenCookieName, out var token)
            || string.IsNullOrWhiteSpace(token)
        )
        {
            throw new UnauthorizedException(AuthErrorCode.InvalidRefreshToken);
        }

        return token;
    }

    /// <summary>
    /// Appends the access and refresh tokens to the response as HttpOnly cookies.
    /// </summary>
    private void AppendAuthCookies(
        string accessToken,
        string refreshToken,
        int accessTokenMinutes = 0,
        int refreshTokenDays = 0
    )
    {
        CookieHelper.Append(
            Response,
            CookieHelper.AccessTokenCookieName,
            accessToken,
            DateTimeOffset.UtcNow.AddMinutes(accessTokenMinutes)
        );

        CookieHelper.Append(
            Response,
            CookieHelper.RefreshTokenCookieName,
            refreshToken,
            DateTimeOffset.UtcNow.AddDays(refreshTokenDays)
        );
    }

    /// <summary>
    /// Clear the access and refresh token cookies from the response, effectively logging out the user.
    /// </summary>
    private void ClearAuthCookies()
    {
        CookieHelper.Delete(Response, CookieHelper.AccessTokenCookieName);
        CookieHelper.Delete(Response, CookieHelper.RefreshTokenCookieName);
    }

    #endregion
}
