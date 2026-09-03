using Microsoft.AspNetCore.Mvc;
using MyTarotReader.Api.Helpers;
using MyTarotReader.Application.Contracts.Services;
using MyTarotReader.Application.Dtos;

namespace MyTarotReader.Api.Controllers;

[ApiController]
[Route("api/v1/ai-tarot")]
public class AITarotController : ControllerBase
{
    private readonly IAiTarotService _service;

    public AITarotController(IAiTarotService service)
    {
        _service = service;
    }

    /// <summary>
    /// Performs an AI tarot reading: validates the drawn cards, asks Gemini to interpret
    /// them against the question, persists the result, and returns the AI answer.
    /// </summary>
    /// <param name="request">The cards, card count, and question for the reading.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The AI-generated interpretation text.</returns>
    /// <response code="200">The reading was generated and saved.</response>
    /// <response code="400">Invalid card code or mismatched card count.</response>
    /// <response code="401">The caller is not authenticated.</response>
    /// <response code="500">The Gemini call failed or returned no answer.</response>
    [HttpPost("reading")]
    [ProducesResponseType(typeof(CreateAiTarotReadingResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    // [Authorize]
    public async Task<IActionResult> CreateAiTarotReadingAsync(
        [FromBody] CreateAiTarotReadingRequest request,
        CancellationToken cancellationToken
    )
    {
        var userId = JwtHelper.GetUserId(HttpContext);

        var result = await _service.CreateAiTarotReadingAsync(request, userId, cancellationToken);
        return Ok(ApiResponse.Success(result));
    }
}
