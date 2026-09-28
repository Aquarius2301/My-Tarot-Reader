using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyTarotReader.Api.Helpers;
using MyTarotReader.Application.Common.Models;
using MyTarotReader.Application.Contracts.Services;

namespace MyTarotReader.Api.Controllers;

[Route("api/aiDeepTarot")]
[ApiController]
[ProducesErrorResponseType(typeof(ApiResponse<object>))]
public class AIDeepTarotReadingController(IAIDeepTarotReadingService service) : ControllerBase
{
    private readonly IAIDeepTarotReadingService _service = service;

    /// <summary>
    /// Creates a new deep tarot reading by generating a reading with Gemini and saving it.
    /// </summary>
    /// <remarks>
    /// The request contains the topic, the locale and the drawn cards (code + reversed status),
    /// whose count must match the topic's spread size (e.g. 12 cards for the 12 houses topic).
    /// The AI-generated answer is persisted only after the AI call succeeds.
    /// </remarks>
    [HttpPut]
    [Authorize]
    [ProducesResponseType(
        typeof(ApiResponse<CreateAiDeepTarotReadingResult>),
        StatusCodes.Status200OK
    )]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateAiDeepTarotReadingAsync(
        [FromBody] CreateAiDeepTarotReadingRequest request,
        CancellationToken cancellationToken
    )
    {
        var userId = JwtHelper.GetUserId(HttpContext);

        var result = await _service.CreateAiDeepTarotReadingAsync(request, userId, cancellationToken);

        return Ok(ApiResponse.Success(result));
    }

    /// <summary>
    /// Retrieves a single deep tarot reading for the authenticated user.
    /// </summary>
    [HttpGet("{readingId:guid}")]
    [Authorize]
    [ProducesResponseType(
        typeof(ApiResponse<GetAiDeepTarotReadingResult>),
        StatusCodes.Status200OK
    )]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAiDeepTarotReadingByIdAsync(
        Guid readingId,
        CancellationToken cancellationToken
    )
    {
        var userId = JwtHelper.GetUserId(HttpContext);

        var result = await _service.GetAiDeepTarotReadingByIdAsync(
            userId,
            readingId,
            cancellationToken
        );

        return Ok(ApiResponse.Success(result));
    }

    /// <summary>
    /// Retrieves all deep tarot readings for the authenticated user.
    /// </summary>
    /// <remarks>
    /// Returns only a short excerpt of each answer (not the full answer).
    /// </remarks>
    [HttpGet]
    [Authorize]
    [ProducesResponseType(
        typeof(ApiResponse<GetAllAiDeepTarotReadingResult>),
        StatusCodes.Status200OK
    )]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAllAiDeepTarotReadingsAsync(
        CancellationToken cancellationToken
    )
    {
        var userId = JwtHelper.GetUserId(HttpContext);

        var result = await _service.GetAllAiDeepTarotReadingsAsync(userId, cancellationToken);

        return Ok(ApiResponse.Success(result));
    }

    /// <summary>
    /// Deletes a specific deep tarot reading for the authenticated user.
    /// </summary>
    [HttpDelete("{readingId:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAiDeepTarotReadingAsync(
        Guid readingId,
        CancellationToken cancellationToken
    )
    {
        var userId = JwtHelper.GetUserId(HttpContext);

        await _service.DeleteAiDeepTarotReadingAsync(userId, readingId, cancellationToken);

        return Ok(ApiResponse.Success());
    }
}
