using Microsoft.AspNetCore.Mvc;
using MyTarotReader.Api.Helpers;
using MyTarotReader.Application.Common.Models;
using MyTarotReader.Application.Contracts.Services;

namespace MyTarotReader.Api.Controllers;

/// <summary>
/// Development-only login that mints real JWTs for a seeded development user so
/// <c>[Authorize]</c> endpoints can be tested from Swagger UI without Google OAuth.
/// </summary>
/// <remarks>
/// Removed from the application model outside the Development environment by
/// <see cref="DevelopmentOnlyControllerConvention"/>.
/// </remarks>
[Route("api/dev/auth")]
[ApiController]
[DevelopmentOnly]
[ProducesErrorResponseType(typeof(ApiResponse<object>))]
public class DevAuthController(IDevAuthService service) : ControllerBase
{
    /// <summary>
    /// Issues an access token for the seeded development user and sets the auth cookies.
    /// </summary>
    /// <remarks>
    /// The request body is optional and may be sent empty. The returned
    /// <c>accessToken</c> can be pasted into Swagger UI's "Authorize" dialog, or the
    /// HttpOnly cookies can be relied on directly. Requires the "X-Device-Id" header
    /// to be set for the refresh token to be bound to a device.
    /// </remarks>
    [HttpPost("token")]
    [ProducesResponseType(typeof(ApiResponse<CreateDevTokenResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateDevTokenAsync(
        [FromBody] CreateDevTokenRequest? request,
        CancellationToken cancellationToken
    )
    {
        var deviceId = Request.Headers["X-Device-Id"].ToString();
        var result = await service.CreateDevTokenAsync(
            request ?? new CreateDevTokenRequest(null),
            deviceId,
            cancellationToken
        );

        CookieHelper.Append(
            Response,
            CookieHelper.AccessTokenCookieName,
            result.AccessToken,
            DateTimeOffset.UtcNow.AddMinutes(result.AccessTokenMinutes)
        );

        CookieHelper.Append(
            Response,
            CookieHelper.RefreshTokenCookieName,
            result.RefreshToken,
            DateTimeOffset.UtcNow.AddDays(result.RefreshTokenDays)
        );

        return Ok(ApiResponse.Success(result));
    }
}
