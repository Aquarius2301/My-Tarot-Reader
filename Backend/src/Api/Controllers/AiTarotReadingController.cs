using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyTarotReader.Api.Helpers;
using MyTarotReader.Application.Common.Models;
using MyTarotReader.Application.Contracts.Services;

namespace MyTarotReader.Api.Controllers;

[Route("api/aiTarot")]
[ApiController]
[ProducesErrorResponseType(typeof(ApiResponse<object>))]
public class AiTarotReadingController(IAiTarotReadingService service) : ControllerBase
{
    private readonly IAiTarotReadingService _service = service;

    /// <summary>
    /// Creates a new AI tarot reading by generating a reading with Gemini and saving it.
    /// </summary>
    /// <remarks>
    /// The request contains the spread configuration (card count, question type, locale)
    /// and the drawn cards (code + reversed status). The AI-generated answer is
    /// persisted only after the AI call succeeds. The ID of the created reading is returned.
    /// </remarks>
    [HttpPut]
    [Authorize]
    [ProducesResponseType(
        typeof(ApiResponse<CreateAiTarotReadingResult>),
        StatusCodes.Status200OK
    )]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateAiTarotReadingAsync(
        [FromBody] CreateAiTarotReadingRequest request,
        CancellationToken cancellationToken
    )
    {
        var userId = JwtHelper.GetUserId(HttpContext);

        var result = await _service.CreateAiTarotReadingAsync(request, userId, cancellationToken);

        return Ok(ApiResponse.Success(result));
    }

    /// <summary>
    /// Retrieves a single AI tarot reading for the authenticated user.
    /// </summary>
    [HttpGet("{readingId:guid}")]
    [Authorize]
    [ProducesResponseType(
        typeof(ApiResponse<GetAiTarotReadingResult>),
        StatusCodes.Status200OK
    )]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAiTarotReadingByIdAsync(
        Guid readingId,
        CancellationToken cancellationToken
    )
    {
        var userId = JwtHelper.GetUserId(HttpContext);

        var result = await _service.GetAiTarotReadingByIdAsync(
            userId,
            readingId,
            cancellationToken
        );

        return Ok(ApiResponse.Success(result));
    }

    /// <summary>
    /// Retrieves all AI tarot readings for the authenticated user.
    /// </summary>
    /// <remarks>
    /// Returns only a short excerpt of each answer (not the full answer).
    /// </remarks>
    [HttpGet]
    [Authorize]
    [ProducesResponseType(
        typeof(ApiResponse<GetAllAiTarotReadingResult>),
        StatusCodes.Status200OK
    )]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAllAiTarotReadingsAsync(
        CancellationToken cancellationToken
    )
    {
        var userId = JwtHelper.GetUserId(HttpContext);

        var result = await _service.GetAllAiTarotReadingsAsync(userId, cancellationToken);

        return Ok(ApiResponse.Success(result));
    }

    /// <summary>
    /// Deletes a specific AI tarot reading for the authenticated user.
    /// </summary>
    [HttpDelete("{readingId:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAiTarotReadingAsync(
        Guid readingId,
        CancellationToken cancellationToken
    )
    {
        var userId = JwtHelper.GetUserId(HttpContext);

        await _service.DeleteAiTarotReadingAsync(userId, readingId, cancellationToken);

        return Ok(ApiResponse.Success());
    }
}