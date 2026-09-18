using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyTarotReader.Api.Helpers;
using MyTarotReader.Application.Common.Models;
using MyTarotReader.Application.Contracts.Services;

namespace MyTarotReader.Api.Controllers;

[Route("api/streak")]
[ApiController]
[ProducesErrorResponseType(typeof(ApiResponse<object>))]
public class StreakController(IStreakService service) : ControllerBase
{
    private readonly IStreakService _service = service;

    /// <summary>
    /// Retrieves the current streak information for the authenticated user.
    /// </summary>
    /// <remarks>
    /// Returns the cycle day (0-6), current and longest streak, whether the streak
    /// saver is still unused this month, and whether the user already checked in today.
    /// Returns default values (all zero/false) when the user has no streak yet.
    /// </remarks>
    [HttpGet]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<GetStreakResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetStreakAsync(CancellationToken cancellationToken)
    {
        var userId = JwtHelper.GetUserId(HttpContext);

        var result = await _service.GetStreakAsync(userId, cancellationToken);
        return Ok(ApiResponse.Success(result));
    }

    /// <summary>
    /// Performs a daily check-in for the authenticated user.
    /// </summary>
    /// <remarks>
    /// Creates a new streak on the first check-in, throws an error when the user
    /// already checked in today, otherwise increments the streak and cycle day.
    /// </remarks>
    [HttpPut("checkin")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<CheckInResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CheckInAsync(CancellationToken cancellationToken)
    {
        var userId = JwtHelper.GetUserId(HttpContext);

        var result = await _service.CheckInAsync(userId, cancellationToken);
        return Ok(ApiResponse.Success(result));
    }
}