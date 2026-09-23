using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyTarotReader.Api.Helpers;
using MyTarotReader.Application.Common.Models;
using MyTarotReader.Application.Contracts.Services;

namespace MyTarotReader.Api.Controllers;

[Route("api/wallet")]
[ApiController]
[ProducesErrorResponseType(typeof(ApiResponse<object>))]
public class WalletController(IWalletService service) : ControllerBase
{
    private readonly IWalletService _service = service;

    /// <summary>
    /// Retrieves the white and red coin balances together with the active white
    /// coin batches ordered by expiry (soonest first).
    /// </summary>
    [HttpGet]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<GetWalletResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetWalletAsync(CancellationToken cancellationToken)
    {
        var userId = JwtHelper.GetUserId(HttpContext);

        var result = await _service.GetWalletAsync(userId, cancellationToken);
        return Ok(ApiResponse.Success(result));
    }

    /// <summary>
    /// Converts red coins into white coins (1 red = 2 white).
    /// </summary>
    /// <remarks>
    /// The granted white coins are added as a new dated batch that expires after
    /// <c>Wallet:ExpireDays</c>. Per the API contract, POST returns
    /// <c>data = null</c>; call <see cref="GetWalletAsync"/> to read the updated state.
    /// </remarks>
    [HttpPost("convert")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ConvertRedToWhiteAsync(
        [FromBody] ConvertRedToWhiteRequest request,
        CancellationToken cancellationToken
    )
    {
        var userId = JwtHelper.GetUserId(HttpContext);

        await _service.ConvertRedToWhiteAsync(userId, request, cancellationToken);
        return Ok(ApiResponse.Success());
    }
}