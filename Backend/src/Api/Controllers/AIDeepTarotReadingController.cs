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
    /// Creates a new 12 astrological houses deep tarot reading by generating a reading with
    /// Gemini and saving it.
    /// </summary>
    /// <remarks>
    /// The request contains the locale and the 12 drawn cards (code + reversed status), one
    /// card per house. The AI-generated answer is persisted only after the AI call succeeds.
    /// The ID of the created reading is returned.
    /// </remarks>
    [HttpPost("twelveHouses")]
    [Authorize]
    [ProducesResponseType(
        typeof(ApiResponse<CreateTwelveHousesReadingResult>),
        StatusCodes.Status200OK
    )]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateTwelveHousesReadingAsync(
        [FromBody] CreateTwelveHousesReadingRequest request,
        CancellationToken cancellationToken
    )
    {
        var userId = JwtHelper.GetUserId(HttpContext);

        var result = await _service.CreateTwelveHousesReadingAsync(
            request,
            userId,
            cancellationToken
        );

        return Ok(ApiResponse.Success(result));
    }

    /// <summary>
    /// Creates a new 12 months deep tarot reading by generating a reading with Gemini and
    /// saving it.
    /// </summary>
    /// <remarks>
    /// The request contains the locale and the 12 drawn cards (code + reversed status), one
    /// card per month. The spread covers the consecutive calendar months starting the month
    /// after the month this reading is created in. The AI-generated answer is persisted only
    /// after the AI call succeeds. The ID of the created reading is returned.
    /// </remarks>
    [HttpPost("twelveMonths")]
    [Authorize]
    [ProducesResponseType(
        typeof(ApiResponse<CreateTwelveMonthsReadingResult>),
        StatusCodes.Status200OK
    )]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateTwelveMonthsReadingAsync(
        [FromBody] CreateTwelveMonthsReadingRequest request,
        CancellationToken cancellationToken
    )
    {
        var userId = JwtHelper.GetUserId(HttpContext);

        var result = await _service.CreateTwelveMonthsReadingAsync(
            request,
            userId,
            cancellationToken
        );

        return Ok(ApiResponse.Success(result));
    }

    /// <summary>
    /// Creates a new crossroads deep tarot reading by generating a reading with Gemini and
    /// saving it.
    /// </summary>
    /// <remarks>
    /// The request contains the locale, the decision question, the 2 to 4 options being
    /// compared, the optional decision timeframe and the drawn cards: three cards per option
    /// plus one closing summary card, so the card count must be <c>options.Count * 3 + 1</c>.
    /// The reading costs one red coin per option, charged only after the AI call succeeds.
    /// The ID of the created reading is returned.
    /// </remarks>
    [HttpPost("crossroads")]
    [Authorize]
    [ProducesResponseType(
        typeof(ApiResponse<CreateCrossroadsReadingResult>),
        StatusCodes.Status200OK
    )]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateCrossroadsReadingAsync(
        [FromBody] CreateCrossroadsReadingRequest request,
        CancellationToken cancellationToken
    )
    {
        var userId = JwtHelper.GetUserId(HttpContext);

        var result = await _service.CreateCrossroadsReadingAsync(
            request,
            userId,
            cancellationToken
        );

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
