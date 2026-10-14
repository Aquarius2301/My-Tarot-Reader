using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyTarotReader.Api.Helpers;
using MyTarotReader.Application.Common.Models;
using MyTarotReader.Application.Features.Shop;
using MyTarotReader.Application.Features.Shop.Commands.CreatePayment;
using PayOS.Models.Webhooks;

namespace MyTarotReader.Api.Controllers;

public record CreatePaymentRequest(string PackageCode);

[Route("api/shop")]
[ApiController]
[ProducesErrorResponseType(typeof(ApiResponse<object>))]
public class ShopController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Retrieves the purchasable red-coin packages.
    /// </summary>
    [HttpGet("packages")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<GetPackagesResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetPackagesAsync(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetPackagesQuery(), cancellationToken);
        return Ok(ApiResponse.Success(result));
    }

    /// <summary>
    /// Creates a payment order and returns the PayOS checkout url to open.
    /// </summary>
    /// <remarks>
    /// Per the API contract this POST returns the created resource (order id and checkout
    /// url) so the client can redirect the buyer to PayOS; poll
    /// <see cref="GetOrderStatusAsync"/> until the order becomes <c>paid</c>.
    /// </remarks>
    [HttpPost("orders")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<CreatePaymentResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateOrderAsync(
        [FromBody] CreatePaymentRequest request,
        CancellationToken cancellationToken
    )
    {
        var userId = JwtHelper.GetUserId(HttpContext);

        var result = await mediator.Send(
            new CreatePaymentCommand(userId, request.PackageCode),
            cancellationToken
        );
        return Ok(ApiResponse.Success(result));
    }

    /// <summary>
    /// Returns the status of one of the user's payment orders; pending orders are
    /// reconciled with PayOS and credited when the payment has settled.
    /// </summary>
    [HttpGet("orders/{id:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<GetOrderStatusResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOrderStatusAsync(
        Guid id,
        CancellationToken cancellationToken
    )
    {
        var userId = JwtHelper.GetUserId(HttpContext);

        var result = await mediator.Send(new GetOrderStatusQuery(id, userId), cancellationToken);
        return Ok(ApiResponse.Success(result));
    }

    /// <summary>
    /// Receives payment notifications from PayOS. The payload is authenticated by its
    /// checksum signature instead of JWT, so the endpoint is anonymous.
    /// </summary>
    /// <remarks>
    /// Returns PayOS's expected acknowledgement format rather than the standard
    /// <see cref="ApiResponse{T}"/> envelope.
    /// </remarks>
    [HttpPost("webhook/payos")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> HandlePayOsWebhookAsync(
        [FromBody] Webhook webhook,
        CancellationToken cancellationToken
    )
    {
        await mediator.Send(new HandleWebhookCommand(webhook), cancellationToken);
        return Ok(new { error = 0, message = "success" });
    }
}
