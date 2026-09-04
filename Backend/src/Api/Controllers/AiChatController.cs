using Microsoft.AspNetCore.Mvc;
using MyTarotReader.Api.Helpers;
using MyTarotReader.Application.Contracts.Services;
using MyTarotReader.Application.Dtos;

namespace MyTarotReader.Api.Controllers;

[ApiController]
[Route("api/v1/ai-chat")]
public class AiChatController : ControllerBase
{
    private readonly IAiChatService _service;

    public AiChatController(IAiChatService service)
    {
        _service = service;
    }

    /// <summary>
    /// Creates a new custom AI tarot chat session with the user's free-text question
    /// and returns the AI's initial response.
    /// </summary>
    /// <param name="request">The chat session creation request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The session id and the AI's initial response.</returns>
    /// <response code="200">The session was created and the AI responded.</response>
    /// <response code="400">The question is empty or invalid.</response>
    /// <response code="401">The caller is not authenticated.</response>
    /// <response code="500">The Gemini call failed or returned no answer.</response>
    [HttpPost("session")]
    [ProducesResponseType(typeof(CreateChatSessionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    // [Authorize]
    public async Task<IActionResult> CreateChatSessionAsync(
        [FromBody] CreateChatSessionRequest request,
        CancellationToken cancellationToken
    )
    {
        var userId = JwtHelper.GetUserId(HttpContext);
        var result = await _service.CreateChatSessionAsync(request, userId, cancellationToken);
        return Ok(ApiResponse.Success(result));
    }

    /// <summary>
    /// Sends a message in an ongoing chat session and returns the AI's response.
    /// The client must send the full conversation history for multi-turn context.
    /// </summary>
    /// <param name="request">The chat message request with conversation history.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The AI's response text.</returns>
    /// <response code="200">The message was processed and the AI responded.</response>
    /// <response code="400">The session is not in chat phase or conversation is invalid.</response>
    /// <response code="401">The caller is not authenticated.</response>
    /// <response code="404">The session was not found.</response>
    /// <response code="500">The Gemini call failed or returned no answer.</response>
    [HttpPost("chat")]
    [ProducesResponseType(typeof(SendChatMessageResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    // [Authorize]
    public async Task<IActionResult> SendChatMessageAsync(
        [FromBody] SendChatMessageRequest request,
        CancellationToken cancellationToken
    )
    {
        var userId = JwtHelper.GetUserId(HttpContext);
        var result = await _service.SendChatMessageAsync(request, userId, cancellationToken);
        return Ok(ApiResponse.Success(result));
    }

    /// <summary>
    /// Submits drawn cards for reading in a custom chat session and returns
    /// the AI's interpretation based on the conversation context and spread positions.
    /// </summary>
    /// <param name="request">The custom reading request with session id and drawn cards.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The AI-generated interpretation text.</returns>
    /// <response code="200">The reading was generated and saved.</response>
    /// <response code="400">Invalid card count, card codes, or session phase.</response>
    /// <response code="401">The caller is not authenticated.</response>
    /// <response code="404">The session was not found.</response>
    /// <response code="500">The Gemini call failed or returned no answer.</response>
    [HttpPost("reading")]
    [ProducesResponseType(typeof(CreateCustomReadingResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    // [Authorize]
    public async Task<IActionResult> CreateCustomReadingAsync(
        [FromBody] CreateCustomReadingRequest request,
        CancellationToken cancellationToken
    )
    {
        var userId = JwtHelper.GetUserId(HttpContext);
        var result = await _service.CreateCustomReadingAsync(request, userId, cancellationToken);
        return Ok(ApiResponse.Success(result));
    }
}
