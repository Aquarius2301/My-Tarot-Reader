using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyTarotReader.Application.Common.Models;
using MyTarotReader.Application.Features.Admin;

namespace MyTarotReader.Api.Controllers;

[Route("api/admin")]
[ApiController]
[ProducesErrorResponseType(typeof(ApiResponse<object>))]
public class AdminController(IMediator mediator) : ControllerBase
{
    /// <summary> Health-check endpoint restricted to Admin users. </summary>
    /// <returns> 200 OK when the caller is an Admin. </returns>
    [HttpGet("ping")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public IActionResult Ping() => Ok(ApiResponse.Success());

    /// <summary> Aggregated tarot reading statistics (guest, registered, AI tarot). </summary>
    /// <returns> 200 OK with the statistic result. </returns>
    [HttpGet("tarot-statistic")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ApiResponse<GetTarotStatisticResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetTarotStatisticAsync(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetTarotStatisticQuery(), cancellationToken);
        return Ok(ApiResponse.Success(result));
    }
}
