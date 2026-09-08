using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyTarotReader.Api.Helpers;
using MyTarotReader.Application.Contracts.Services;
using MyTarotReader.Application.Dtos;

namespace MyTarotReader.Api.Controllers;

[ApiController]
[Route("api/v1/history")]
public class HistoryController : ControllerBase
{
    private readonly IHistoryService _service;

    public HistoryController(IHistoryService service)
    {
        _service = service;
    }

    /// <summary>
    /// Retrieves all active read‑history items for the authenticated user, ordered by creation date.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of read‑history DTOs.</returns>
    /// <response code="200">The history list was returned successfully.</response>
    /// <response code="401">The caller is not authenticated.</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [Authorize]
    public async Task<IActionResult> GetHistoryAsync(CancellationToken cancellationToken)
    {
        var userId = JwtHelper.GetUserId(HttpContext);
        var history = await _service.GetHistoryAsync(userId, cancellationToken);
        return Ok(ApiResponse.Success(history));
    }

    /// <summary>
    /// Soft-deletes a single read‑history item for the authenticated user by setting its DeletedAt timestamp.
    /// </summary>
    /// <param name="historyId">The ID of the history item to delete.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A success response.</returns>
    /// <response code="200">The history item was deleted successfully.</response>
    /// <response code="401">The caller is not authenticated.</response>
    [HttpDelete("{historyId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [Authorize]
    public async Task<IActionResult> DeleteHistoryAsync(
        Guid historyId,
        CancellationToken cancellationToken
    )
    {
        var userId = JwtHelper.GetUserId(HttpContext);

        await _service.DeleteHistoryAsync(userId, historyId, cancellationToken);
        return Ok(ApiResponse.Success());
    }

    /// <summary>
    /// Retrieves all active AI read-history items for the authenticated user, ordered by creation date.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A list of AI read-history DTOs.</returns>
    /// <response code="200">The AI history list was returned successfully.</response>
    /// <response code="401">The caller is not authenticated.</response>
    [HttpGet("ai")]
    [ProducesResponseType(typeof(List<AIReadHistoryResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [Authorize]
    public async Task<IActionResult> GetAllAiReadHistoryAsync(CancellationToken cancellationToken)
    {
        var userId = JwtHelper.GetUserId(HttpContext);
        var history = await _service.GetAllAiReadHistoryAsync(userId, cancellationToken);
        return Ok(ApiResponse.Success(history));
    }

    /// <summary>
    /// Soft-deletes a single AI read-history item for the authenticated user by setting its DeletedAt timestamp.
    /// </summary>
    /// <param name="historyId">The ID of the AI read-history item to delete.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A success response.</returns>
    /// <response code="200">The AI history item was deleted successfully.</response>
    /// <response code="401">The caller is not authenticated.</response>
    /// <response code="404">The AI history item was not found or does not belong to this user.</response>
    [HttpDelete("ai/{historyId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Authorize]
    public async Task<IActionResult> DeleteAiReadHistoryAsync(
        Guid historyId,
        CancellationToken cancellationToken
    )
    {
        var userId = JwtHelper.GetUserId(HttpContext);
        await _service.DeleteAiReadHistoryAsync(userId, historyId, cancellationToken);
        return Ok(ApiResponse.Success());
    }
}
