using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyTarotReader.Api.Helpers;
using MyTarotReader.Application.Common.Models;
using MyTarotReader.Application.Features.AiDeepTarot;
using MyTarotReader.Domain.Enums;

namespace MyTarotReader.Api.Controllers;

public record CreateTwelveHousesReadingRequest(string Locale, List<AiDeepCardRequest> Cards);

public record CreateTwelveMonthsReadingRequest(string Locale, List<AiDeepCardRequest> Cards);

public record CreateCrossroadsReadingRequest(
    string Locale,
    string Question,
    List<string> Options,
    CrossroadsTimeFrame? TimeFrame,
    List<AiDeepCardRequest> Cards
);

public record GetAllAiDeepTarotReadingRequest(int Page, int PageSize);

[Route("api/aiDeepTarot")]
[ApiController]
[ProducesErrorResponseType(typeof(ApiResponse<object>))]
public class AIDeepTarotReadingController(IMediator mediator) : ControllerBase
{
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
    [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateTwelveHousesReadingAsync(
        [FromBody] CreateTwelveHousesReadingRequest request,
        CancellationToken cancellationToken
    )
    {
        var userId = JwtHelper.GetUserId(HttpContext);

        var readingId = await mediator.Send(
            new CreateTwelveHousesReadingCommand(request.Locale, request.Cards, userId),
            cancellationToken
        );

        return Ok(ApiResponse.Success(readingId));
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
    [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateTwelveMonthsReadingAsync(
        [FromBody] CreateTwelveMonthsReadingRequest request,
        CancellationToken cancellationToken
    )
    {
        var userId = JwtHelper.GetUserId(HttpContext);

        var readingId = await mediator.Send(
            new CreateTwelveMonthsReadingCommand(request.Locale, request.Cards, userId),
            cancellationToken
        );

        return Ok(ApiResponse.Success(readingId));
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
    [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateCrossroadsReadingAsync(
        [FromBody] CreateCrossroadsReadingRequest request,
        CancellationToken cancellationToken
    )
    {
        var userId = JwtHelper.GetUserId(HttpContext);

        var readingId = await mediator.Send(
            new CreateCrossroadsReadingCommand(
                request.Locale,
                request.Question,
                request.Options,
                request.TimeFrame,
                request.Cards,
                userId
            ),
            cancellationToken
        );

        return Ok(ApiResponse.Success(readingId));
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

        var result = await mediator.Send(
            new GetAiDeepTarotReadingByIdQuery(userId, readingId),
            cancellationToken
        );

        return Ok(ApiResponse.Success(result));
    }

    /// <summary>
    /// Retrieves a page of deep tarot readings for the authenticated user.
    /// </summary>
    /// <remarks>
    /// Returns only a short excerpt of each answer (not the full answer).
    /// </remarks>
    [HttpGet]
    [Authorize]
    [ProducesResponseType(
        typeof(ApiResponse<GetAllAiDeepTarotReadingsResult>),
        StatusCodes.Status200OK
    )]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetAllAiDeepTarotReadingsAsync(
        [FromQuery] GetAllAiDeepTarotReadingRequest request,
        CancellationToken cancellationToken
    )
    {
        var userId = JwtHelper.GetUserId(HttpContext);

        var result = await mediator.Send(
            new GetAllAiDeepTarotReadingsQuery(request.Page, request.PageSize, userId),
            cancellationToken
        );

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

        await mediator.Send(
            new DeleteAiDeepTarotReadingCommand(userId, readingId),
            cancellationToken
        );

        return Ok(ApiResponse.Success());
    }
}
