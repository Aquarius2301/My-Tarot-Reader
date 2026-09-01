using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyTarotReader.Api.Helpers;
using MyTarotReader.Application.Contracts.Services;
using MyTarotReader.Application.Dtos;

namespace MyTarotReader.Api.Controllers;

[ApiController]
[Route("api/v1/tarot")]
public class TarotController : ControllerBase
{
    private readonly ITarotService _service;

    public TarotController(ITarotService service)
    {
        _service = service;
    }

    /// <summary>
    /// Gets the last drawn card for the authenticated user.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A response containing the last drawn card (or null if none).</returns>
    /// <response code="200">The last drawn card was returned successfully.</response>
    /// <response code="401">The caller is not authenticated.</response>
    [HttpGet("draw")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [Authorize]
    public async Task<IActionResult> GetLastDrawnCardForAuthAsync(
        CancellationToken cancellationToken
    )
    {
        var userId = JwtHelper.GetUserId(HttpContext);

        var result = await _service.GetLastDrawnCardForAuthAsync(userId, cancellationToken);
        return Ok(ApiResponse.Success(result));
    }

    /// <summary>
    /// Persists a read-history entry for the authenticated user.
    /// </summary>
    /// <param name="request">DTO containing the drawn card code.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A success response.</returns>
    /// <response code="200">The draw was saved successfully.</response>
    /// <response code="400">The request body is invalid.</response>
    /// <response code="401">The caller is not authenticated.</response>
    [HttpPost("draw")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [Authorize]
    public async Task<IActionResult> CreateDrawForAuthAsync(
        [FromBody] CreateDrawForAuthRequest request,
        CancellationToken cancellationToken
    )
    {
        var userId = JwtHelper.GetUserId(HttpContext);
        await _service.CreateDrawForAuthAsync(
            request.CardCode,
            request.IsReversed,
            userId,
            cancellationToken
        );
        return Ok(ApiResponse.Success());
    }

    /// <summary>
    /// Returns the guest draw status and remaining cooldown for the supplied guest key.
    /// </summary>
    /// <param name="guestKey">The guest key stored in the cookie.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A response containing the current guest draw state.</returns>
    /// <response code="200">The guest draw status was returned successfully.</response>
    /// <response code="500">An unexpected server error occurred.</response>
    [HttpGet("guest-draw")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetLastDrawnCardForGuestAsync(
        [FromQuery] string guestKey,
        CancellationToken cancellationToken
    )
    {
        var availability = await _service.GetLastDrawnCardForGuestAsync(
            guestKey,
            cancellationToken
        );
        return Ok(ApiResponse.Success(availability));
    }

    /// <summary>
    /// Stores a guest draw and sets the cookie used to clear it from Swagger.
    /// </summary>
    /// <param name="request">The guest draw request payload.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A success response.</returns>
    /// <response code="200">The guest draw was saved successfully.</response>
    /// <response code="400">The request body is invalid.</response>
    /// <response code="429">The guest has already drawn within the cooldown window.</response>
    /// <response code="500">An unexpected server error occurred.</response>
    [HttpPost("guest-draw")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateDrawForGuestAsync(
        [FromBody] CreateDrawForGuestRequest request,
        CancellationToken cancellationToken
    )
    {
        CookieHelper.Append(
            Response,
            CookieHelper.GuestCookieName,
            request.GuestKey,
            DateTimeOffset.UtcNow.AddDays(1)
        ); // Save the guest key in a cookie for easy to remove key on swagger (testing)

        await _service.CreateDrawForGuestAsync(
            request.GuestKey,
            request.CardCode,
            request.IsReversed,
            cancellationToken
        );

        return Ok(ApiResponse.Success());
    }

    /// <summary>
    /// Clears the guest draw so the guest can draw again.
    /// </summary>
    /// <param name="guestKey">The guest key to remove.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A success response.</returns>
    /// <response code="200">The guest draw was cleared successfully.</response>
    /// <response code="400">The guest key is invalid.</response>
    /// <response code="500">An unexpected server error occurred.</response>
    [HttpDelete("guest-draw")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ClearGuestDrawAsync(
        [FromQuery] string guestKey,
        CancellationToken cancellationToken
    )
    {
        await _service.RemoveDrawForGuestAsync(guestKey, cancellationToken);
        return Ok(ApiResponse.Success());
    }
}
