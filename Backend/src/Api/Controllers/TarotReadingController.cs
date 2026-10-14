using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyTarotReader.Api.Helpers;
using MyTarotReader.Application.Common.Models;
using MyTarotReader.Application.Features.Tarot;

namespace MyTarotReader.Api.Controllers;

public record CreateDrawForAuthRequest(string CardCode, bool IsReversed);

public record CreateDrawForGuestRequest(string CardCode, bool IsReversed);

public record GetAllReadingRequest(int Page, int PageSize);

[Route("api/tarot")]
[ApiController]
[ProducesErrorResponseType(typeof(ApiResponse<object>))]
public class TarotReadingController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Retrieves the last drawn tarot card for an authenticated user.
    /// </summary>
    [HttpGet("draw")]
    [Authorize]
    [ProducesResponseType(
        typeof(ApiResponse<GetLastDrawnCardForAuthResult>),
        StatusCodes.Status200OK
    )]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetLastDrawnCardForAuthAsync(
        CancellationToken cancellationToken
    )
    {
        var userId = JwtHelper.GetUserId(HttpContext);

        var result = await mediator.Send(
            new GetLastDrawnCardForAuthQuery(userId),
            cancellationToken
        );
        return Ok(ApiResponse.Success(result));
    }

    /// <summary>
    /// Creates a new tarot card draw for an authenticated user.
    /// </summary>
    [HttpPost("draw")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateDrawForAuthAsync(
        [FromBody] CreateDrawForAuthRequest request,
        CancellationToken cancellationToken
    )
    {
        var userId = JwtHelper.GetUserId(HttpContext);

        await mediator.Send(
            new CreateDrawForAuthCommand(request.CardCode, request.IsReversed, userId),
            cancellationToken
        );
        return Ok(ApiResponse.Success());
    }

    /// <summary>
    /// Retrieves the last drawn tarot card for a guest user.
    /// </summary>
    /// <remarks>
    /// The card is saved in Redis with an initial cooldown.
    /// Use the "X-Device-Id" header to provide a unique identifier for the guest user.
    /// </remarks>
    [HttpGet("guest-draw")]
    [ProducesResponseType(
        typeof(ApiResponse<GetLastDrawnCardForGuestResult>),
        StatusCodes.Status200OK
    )]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetLastDrawnCardForGuestAsync(
        CancellationToken cancellationToken
    )
    {
        var guestKey = Request.Headers["X-Device-Id"].ToString();

        var result = await mediator.Send(
            new GetLastDrawnCardForGuestQuery(guestKey),
            cancellationToken
        );

        return Ok(ApiResponse.Success(result));
    }

    /// <summary>
    /// Creates a new tarot card draw for a guest user.
    /// </summary>
    /// <remarks>
    /// The card is saved in Redis with an initial cooldown.
    /// Use the "X-Device-Id" header to provide a unique identifier for the guest user.
    /// </remarks>
    [HttpPost("guest-draw")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> CreateDrawForGuestAsync(
        [FromBody] CreateDrawForGuestRequest request,
        CancellationToken cancellationToken
    )
    {
        var guestKey = Request.Headers["X-Device-Id"].ToString();

        CookieHelper.Append(
            Response,
            CookieHelper.GuestCookieName,
            guestKey,
            DateTimeOffset.UtcNow.AddDays(1)
        ); // Save the guest key in a cookie for easy to remove key on swagger (testing)

        await mediator.Send(
            new CreateDrawForGuestCommand(request.CardCode, request.IsReversed, guestKey),
            cancellationToken
        );

        return Ok(ApiResponse.Success());
    }

    /// <summary>
    /// Clears the last drawn tarot card for a guest user.
    /// </summary>
    /// <remarks> This endpoint is for testing purposes only. </remarks>
    [HttpDelete("guest-draw")]
    [DevelopmentOnly]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ClearGuestDrawAsync(
        [FromQuery] string guestKey,
        CancellationToken cancellationToken
    )
    {
        await mediator.Send(new RemoveDrawnForGuestCommand(guestKey), cancellationToken);
        return Ok(ApiResponse.Success());
    }

    /// <summary>
    /// Retrieves a page of tarot readings for the authenticated user.
    /// </summary>
    [HttpGet]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<GetAllReadingResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAllReadingAsync(
        [FromQuery] GetAllReadingRequest request,
        CancellationToken cancellationToken
    )
    {
        var userId = JwtHelper.GetUserId(HttpContext);

        var result = await mediator.Send(
            new GetAllReadingQuery(request.Page, request.PageSize, userId),
            cancellationToken
        );

        return Ok(ApiResponse.Success(result));
    }

    /// <summary>
    /// Deletes a specific reading entry for the authenticated user.
    /// </summary>
    [HttpDelete("{readingId:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteReadingAsync(
        Guid readingId,
        CancellationToken cancellationToken
    )
    {
        var userId = JwtHelper.GetUserId(HttpContext);

        await mediator.Send(new RemoveDrawnForAuthCommand(userId, readingId), cancellationToken);

        return Ok(ApiResponse.Success());
    }
}
