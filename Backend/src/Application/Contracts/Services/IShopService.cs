using PayOS.Models.Webhooks;

namespace MyTarotReader.Application.Contracts.Services;

/// <summary>
/// A purchasable red-coin package exposed by the shop.
/// </summary>
/// <param name="Code">The unique package code used when creating an order.</param>
/// <param name="RedCoins">The number of red coins granted on payment.</param>
/// <param name="PriceVnd">The package price in VND.</param>
public record GetPackagesItem(string Code, int RedCoins, int PriceVnd);

/// <summary>Result of listing the purchasable packages.</summary>
/// <param name="Packages">The available packages.</param>
public record GetPackagesResult(List<GetPackagesItem> Packages);

/// <summary>Request to create a payment order for a shop package.</summary>
/// <param name="PackageCode">The code of the package to purchase (e.g. <c>pack-30</c>).</param>
public record CreatePaymentRequest(string PackageCode);

/// <summary>
/// Result of creating a payment order.
/// </summary>
/// <param name="OrderId">The local payment order id used for status polling.</param>
/// <param name="OrderCode">The PayOS order code.</param>
/// <param name="CheckoutUrl">The PayOS checkout url the client must open to pay.</param>
/// <param name="QrCode">The base64 QR code image of the payment link.</param>
/// <param name="AmountVnd">The order price in VND.</param>
/// <param name="RedCoins">The number of red coins granted on payment.</param>
public record CreatePaymentResult(
    Guid OrderId,
    long OrderCode,
    string CheckoutUrl,
    string QrCode,
    int AmountVnd,
    int RedCoins
);

/// <summary>Request to query the status of a payment order owned by the user.</summary>
/// <param name="Id">The payment order id returned by <see cref="CreatePaymentResult"/>.</param>
public record GetOrderStatusRequest(Guid Id);

/// <summary>
/// Result of querying a payment order status.
/// </summary>
/// <param name="Id">The payment order id.</param>
/// <param name="OrderCode">The PayOS order code.</param>
/// <param name="Status">One of: <c>pending</c>, <c>paid</c>, <c>cancelled</c>, <c>expired</c>, <c>failed</c>.</param>
/// <param name="AmountVnd">The order price in VND.</param>
/// <param name="RedCoins">The number of red coins granted on payment.</param>
/// <param name="PaidAt">The time the payment was confirmed, or <c>null</c> while unpaid.</param>
public record GetOrderStatusResult(
    Guid Id,
    long OrderCode,
    string Status,
    int AmountVnd,
    int RedCoins,
    DateTimeOffset? PaidAt
);

/// <summary>Request wrapping a raw PayOS webhook payload.</summary>
/// <param name="Webhook">The webhook payload received from PayOS.</param>
public record HandleWebhookRequest(Webhook Webhook);

/// <summary>Result of handling a PayOS webhook.</summary>
/// <param name="Processed">Whether a pending order was credited; <c>false</c> for already-handled or ignorable payloads.</param>
public record HandleWebhookResult(bool Processed);

public interface IShopService
{
    /// <summary>Returns the purchasable red-coin packages.</summary>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns><see cref="GetPackagesResult"/> with the configured packages.</returns>
    Task<GetPackagesResult> GetPackagesAsync(CancellationToken cancellationToken = default);

    /// <summary>Creates a payment order and its PayOS payment link.</summary>
    /// <param name="userId">The authenticated user's ID.</param>
    /// <param name="request"><see cref="CreatePaymentRequest"/> containing the package code.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <exception cref="Common.Exceptions.BadRequestException">Thrown when the package code is unknown.</exception>
    /// <exception cref="Common.Exceptions.InternalServerException">Thrown when the PayOS payment link cannot be created.</exception>
    /// <returns><see cref="CreatePaymentResult"/> with the checkout url the client must open.</returns>
    Task<CreatePaymentResult> CreatePaymentAsync(
        Guid userId,
        CreatePaymentRequest request,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Returns the status of one of the user's payment orders, reconciling with PayOS
    /// (and crediting the red coins) when the order is still pending.
    /// </summary>
    /// <param name="userId">The authenticated user's ID.</param>
    /// <param name="request"><see cref="GetOrderStatusRequest"/> containing the order id.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <exception cref="Common.Exceptions.NotFoundException">Thrown when the order does not belong to the user.</exception>
    /// <returns><see cref="GetOrderStatusResult"/> with the current order status.</returns>
    Task<GetOrderStatusResult> GetOrderStatusAsync(
        Guid userId,
        GetOrderStatusRequest request,
        CancellationToken cancellationToken = default
    );

    /// <summary>Verifies and processes a payment notification webhook from PayOS.</summary>
    /// <param name="request"><see cref="HandleWebhookRequest"/> containing the raw webhook payload.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <exception cref="Common.Exceptions.UnauthorizedException">Thrown when the webhook signature is invalid.</exception>
    /// <returns><see cref="HandleWebhookResult"/> describing whether coins were credited.</returns>
    /// <remarks>Idempotent: repeated notifications for an already-paid order are acknowledged without crediting.</remarks>
    Task<HandleWebhookResult> HandleWebhookAsync(
        HandleWebhookRequest request,
        CancellationToken cancellationToken = default
    );
}
