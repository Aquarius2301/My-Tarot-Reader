using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MyTarotReader.Api.Helpers;
using MyTarotReader.Application.Contracts.Services;
using MyTarotReader.Application.Dtos;
using MyTarotReader.Application.Exceptions;
using MyTarotReader.Application.Settings;

namespace MyTarotReader.Api.Controllers;

[Route("api/v1/auth")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IAuthService _service;
    private readonly JwtSetting _jwtSetting;

    public AuthController(IAuthService service, IOptions<JwtSetting> jwtSetting)
    {
        _service = service;
        _jwtSetting = jwtSetting.Value;
    }

    /// <summary>
    /// Authenticates a user with Google OAuth and writes the issued tokens into HttpOnly cookies.
    /// </summary>
    /// <param name="request">The Google credential payload.</param>
    /// <param name="cancellationToken">Cancellation token from the request pipeline.</param>
    /// <returns>A success envelope.</returns>
    /// <response code="200">Authentication succeeded.</response>
    /// <response code="400">The credential is invalid.</response>
    [HttpPost("oauth")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GoogleLoginAsync(
        [FromBody] GoogleLoginRequest request,
        CancellationToken cancellationToken
    )
    {
        var deviceId = Request.Headers["X-Device-Id"].ToString();
        var response = await _service.GoogleLoginAsync(
            request.Credential,
            deviceId,
            cancellationToken
        );

        AppendAuthCookies(response.AccessToken, response.RefreshToken);

        // Tokens are delivered via HttpOnly cookies; the body returns only the user.
        return Ok(ApiResponse.Success());
    }

    /// <summary>
    /// Rotates the refresh token and issues a new access token pair.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token from the request pipeline.</param>
    /// <returns>A success envelope.</returns>
    /// <response code="200">Token refresh succeeded.</response>
    /// <response code="401">The refresh token is missing, expired, or invalid.</response>
    [HttpPost("refresh")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RefreshAsync(CancellationToken cancellationToken)
    {
        var refreshToken = ReadRefreshTokenOrThrow();
        var deviceId = Request.Headers["X-Device-Id"].ToString();

        var response = await _service.RefreshAsync(refreshToken, deviceId, cancellationToken);

        AppendAuthCookies(response.AccessToken, response.RefreshToken);

        return Ok(ApiResponse.Success());
    }

    /// <summary>
    /// Revokes the current refresh token and clears the auth cookies.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token from the request pipeline.</param>
    /// <returns>A success envelope.</returns>
    /// <response code="200">Logout succeeded.</response>
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> LogoutAsync(CancellationToken cancellationToken)
    {
        var refreshToken = Request.Cookies.TryGetValue(
            CookieHelper.RefreshTokenCookieName,
            out var token
        )
            ? token
            : string.Empty;

        await _service.LogoutAsync(refreshToken, cancellationToken);

        ClearAuthCookies();

        return Ok(ApiResponse.Success());
    }

    /// <summary>
    /// Returns the authenticated user's profile and wallet information.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token from the request pipeline.</param>
    /// <returns>A success envelope containing the current user.</returns>
    /// <response code="200">The user profile was returned successfully.</response>
    /// <response code="401">The caller is not authenticated.</response>
    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCurrentUserAsync(CancellationToken cancellationToken)
    {
        var userId = JwtHelper.GetUserId(HttpContext);

        var user = await _service.GetCurrentUserAsync(userId, cancellationToken);

        return Ok(ApiResponse.Success(user));
    }

    private string ReadRefreshTokenOrThrow()
    {
        if (
            !Request.Cookies.TryGetValue(CookieHelper.RefreshTokenCookieName, out var token)
            || string.IsNullOrWhiteSpace(token)
        )
        {
            throw new UnauthorizedException(ErrorMessageCode.Auth.RefreshTokenInvalid);
        }

        return token;
    }

    /// <summary>Writes the access and refresh tokens into HttpOnly cookies, mirroring the guest cookie pattern.</summary>
    private void AppendAuthCookies(string accessToken, string refreshToken)
    {
        CookieHelper.Append(
            Response,
            CookieHelper.AccessTokenCookieName,
            accessToken,
            DateTimeOffset.UtcNow.AddMinutes(_jwtSetting.AccessTokenDurationMinutes)
        );

        CookieHelper.Append(
            Response,
            CookieHelper.RefreshTokenCookieName,
            refreshToken,
            DateTimeOffset.UtcNow.AddDays(_jwtSetting.RefreshTokenDurationDays)
        );
    }

    /// <summary>Clears the auth cookies so the client no longer presents tokens.</summary>
    private void ClearAuthCookies()
    {
        CookieHelper.Delete(Response, CookieHelper.AccessTokenCookieName);
        CookieHelper.Delete(Response, CookieHelper.RefreshTokenCookieName);
    }
}
