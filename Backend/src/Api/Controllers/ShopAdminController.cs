using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyTarotReader.Application.Common.Models;
using MyTarotReader.Application.Features.Shop;

namespace MyTarotReader.Api.Controllers;

public record CreateShopPackageRequest(
    string Code,
    int RedCoins,
    int WhiteCoins,
    int OldPrice,
    int Price,
    bool IsEnabled
);

public record UpdateShopPackageRequest(
    string Code,
    int RedCoins,
    int WhiteCoins,
    int OldPrice,
    int Price,
    bool IsEnabled
);

[Route("api/admin/shop")]
[ApiController]
[Authorize(Roles = "Admin")]
[ProducesErrorResponseType(typeof(ApiResponse<object>))]
public class ShopAdminController(IMediator mediator) : ControllerBase
{
    /// <summary> Lists every shop package, including disabled ones. </summary>
    [HttpGet("packages")]
    [ProducesResponseType(
        typeof(ApiResponse<GetAllShopPackagesResult>),
        StatusCodes.Status200OK
    )]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetPackagesAsync(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetAllShopPackagesQuery(), cancellationToken);
        return Ok(ApiResponse.Success(result));
    }

    /// <summary>
    /// Creates a shop package with a lowercase code, reviving a soft-deleted package with the same
    /// code, and returns its id.
    /// </summary>
    [HttpPost("packages")]
    [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreatePackageAsync(
        [FromBody] CreateShopPackageRequest request,
        CancellationToken cancellationToken
    )
    {
        var id = await mediator.Send(
            new CreateShopPackageCommand(
                request.Code,
                request.RedCoins,
                request.WhiteCoins,
                request.OldPrice,
                request.Price,
                request.IsEnabled
            ),
            cancellationToken
        );

        return Ok(ApiResponse.Success(id));
    }

    /// <summary> Updates an existing shop package, normalizing its code to lowercase. </summary>
    [HttpPut("packages/{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdatePackageAsync(
        Guid id,
        [FromBody] UpdateShopPackageRequest request,
        CancellationToken cancellationToken
    )
    {
        await mediator.Send(
            new UpdateShopPackageCommand(
                id,
                request.Code,
                request.RedCoins,
                request.WhiteCoins,
                request.OldPrice,
                request.Price,
                request.IsEnabled
            ),
            cancellationToken
        );

        return Ok(ApiResponse.Success());
    }

    /// <summary> Soft-deletes a shop package. </summary>
    [HttpDelete("packages/{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeletePackageAsync(
        Guid id,
        CancellationToken cancellationToken
    )
    {
        await mediator.Send(new DeleteShopPackageCommand(id), cancellationToken);
        return Ok(ApiResponse.Success());
    }
}
