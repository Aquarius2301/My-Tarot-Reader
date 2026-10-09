using PayOS.Models;
using PayOS.Models.V2.PaymentRequests;
using PayOS.Models.Webhooks;

namespace MyTarotReader.Application.Contracts.Common;

/// <summary>
/// Thin wrapper around the official PayOS SDK client used to create payment links,
/// query payment status and verify payment webhooks.
/// </summary>
public interface IPayOsClient
{
    /// <summary>Creates a PayOS payment link for an order.</summary>
    /// <param name="request">The payment link details (order code, amount, urls...).</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <exception cref="MyTarotReader.Application.Common.Exceptions.InternalServerException">
    /// Thrown when the PayOS credentials are missing or the PayOS API call fails.
    /// </exception>
    /// <returns>The created payment link (checkout url, qr code, payment link id...).</returns>
    Task<CreatePaymentLinkResponse> CreatePaymentLinkAsync(
        CreatePaymentLinkRequest request,
        CancellationToken cancellationToken = default
    );

    /// <summary>Retrieves the current status of a PayOS payment link by order code.</summary>
    /// <param name="orderCode">The PayOS order code.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <exception cref="MyTarotReader.Application.Common.Exceptions.InternalServerException">
    /// Thrown when the PayOS credentials are missing or the PayOS API call fails.
    /// </exception>
    /// <returns>The payment link information, including its status.</returns>
    Task<PaymentLink> GetPaymentLinkInformationAsync(
        long orderCode,
        CancellationToken cancellationToken = default
    );

    /// <summary>Verifies the signature of a webhook payload received from PayOS.</summary>
    /// <param name="webhook">The raw webhook payload bound from the request body.</param>
    /// <exception cref="MyTarotReader.Application.Common.Exceptions.UnauthorizedException">
    /// Thrown when the payload signature is missing or does not match the checksum key.
    /// </exception>
    /// <returns>The verified webhook data.</returns>
    Task<WebhookData> VerifyWebhookAsync(Webhook webhook);

    /// <summary>Registers (confirms) the webhook URL so PayOS sends payment notifications to it.</summary>
    /// <param name="webhookUrl">The public webhook endpoint URL.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <exception cref="MyTarotReader.Application.Common.Exceptions.InternalServerException">
    /// Thrown when the webhook URL is rejected by PayOS.
    /// </exception>
    Task ConfirmWebhookAsync(string webhookUrl, CancellationToken cancellationToken = default);
}
