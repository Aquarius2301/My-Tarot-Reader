using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyTarotReader.Application.Common.Models;
using MyTarotReader.Application.Features.AiTarot;

namespace MyTarotReader.Api.Controllers;

public record CreateAiTarotReadingCountRequest(
    int CardCount,
    int WhiteCoins,
    List<string> PositionsEn,
    List<string> PositionsVi,
    int MinWords,
    int MaxWords,
    bool IsEnabled
);

public record UpdateAiTarotReadingCountRequest(
    int CardCount,
    int WhiteCoins,
    List<string> PositionsEn,
    List<string> PositionsVi,
    int MinWords,
    int MaxWords,
    bool IsEnabled
);

public record CreateAiTarotReadingTypeRequest(
    string Code,
    string NameEn,
    string NameVi,
    bool IsEnabled
);

public record UpdateAiTarotReadingTypeRequest(
    string Code,
    string NameEn,
    string NameVi,
    bool IsEnabled
);

[Route("api/admin/ai-tarot")]
[ApiController]
[Authorize(Roles = "Admin")]
[ProducesErrorResponseType(typeof(ApiResponse<object>))]
public class AiTarotAdminController(IMediator mediator) : ControllerBase
{
    /// <summary> Lists every AI tarot setting, including disabled entries. </summary>
    [HttpGet("settings")]
    [ProducesResponseType(
        typeof(ApiResponse<GetAllAiTarotSettingsForAdminResult>),
        StatusCodes.Status200OK
    )]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetSettingsAsync(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new GetAllAiTarotSettingsForAdminQuery(),
            cancellationToken
        );
        return Ok(ApiResponse.Success(result));
    }

    /// <summary> Creates a spread configuration and returns its id. </summary>
    [HttpPost("reading-counts")]
    [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateReadingCountAsync(
        [FromBody] CreateAiTarotReadingCountRequest request,
        CancellationToken cancellationToken
    )
    {
        var id = await mediator.Send(
            new CreateAiTarotReadingCountCommand(
                request.CardCount,
                request.WhiteCoins,
                request.PositionsEn,
                request.PositionsVi,
                request.MinWords,
                request.MaxWords,
                request.IsEnabled
            ),
            cancellationToken
        );

        return Ok(ApiResponse.Success(id));
    }

    /// <summary> Updates an existing spread configuration. </summary>
    [HttpPut("reading-counts/{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateReadingCountAsync(
        Guid id,
        [FromBody] UpdateAiTarotReadingCountRequest request,
        CancellationToken cancellationToken
    )
    {
        await mediator.Send(
            new UpdateAiTarotReadingCountCommand(
                id,
                request.CardCount,
                request.WhiteCoins,
                request.PositionsEn,
                request.PositionsVi,
                request.MinWords,
                request.MaxWords,
                request.IsEnabled
            ),
            cancellationToken
        );

        return Ok(ApiResponse.Success());
    }

    /// <summary> Soft-deletes a spread configuration. </summary>
    [HttpDelete("reading-counts/{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteReadingCountAsync(
        Guid id,
        CancellationToken cancellationToken
    )
    {
        await mediator.Send(new DeleteAiTarotReadingCountCommand(id), cancellationToken);
        return Ok(ApiResponse.Success());
    }

    /// <summary> Creates a question type and returns its id. </summary>
    [HttpPost("reading-types")]
    [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateReadingTypeAsync(
        [FromBody] CreateAiTarotReadingTypeRequest request,
        CancellationToken cancellationToken
    )
    {
        var id = await mediator.Send(
            new CreateAiTarotReadingTypeCommand(
                request.Code,
                request.NameEn,
                request.NameVi,
                request.IsEnabled
            ),
            cancellationToken
        );

        return Ok(ApiResponse.Success(id));
    }

    /// <summary> Updates an existing question type. </summary>
    [HttpPut("reading-types/{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateReadingTypeAsync(
        Guid id,
        [FromBody] UpdateAiTarotReadingTypeRequest request,
        CancellationToken cancellationToken
    )
    {
        await mediator.Send(
            new UpdateAiTarotReadingTypeCommand(
                id,
                request.Code,
                request.NameEn,
                request.NameVi,
                request.IsEnabled
            ),
            cancellationToken
        );

        return Ok(ApiResponse.Success());
    }

    /// <summary> Soft-deletes a question type. </summary>
    [HttpDelete("reading-types/{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteReadingTypeAsync(
        Guid id,
        CancellationToken cancellationToken
    )
    {
        await mediator.Send(new DeleteAiTarotReadingTypeCommand(id), cancellationToken);
        return Ok(ApiResponse.Success());
    }
}
