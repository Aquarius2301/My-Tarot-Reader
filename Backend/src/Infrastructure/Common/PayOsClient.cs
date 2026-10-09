using Microsoft.Extensions.Options;
using MyTarotReader.Application.Common.Exceptions;
using MyTarotReader.Application.Constants.Errors;
using MyTarotReader.Application.Contracts.Common;
using MyTarotReader.Application.Settings;
using PayOS;
using PayOS.Models;
using PayOS.Models.V2.PaymentRequests;
using PayOS.Models.Webhooks;
using PayOsException = PayOS.Exceptions.PayOSException;
using WebhookException = PayOS.Exceptions.WebhookException;

namespace MyTarotReader.Infrastructure.Common;

/// <summary>
/// Wraps the official PayOS SDK client, configured from <see cref="PayOsSetting"/>
/// and backed by the injected <see cref="HttpClient"/>.
/// </summary>
public class PayOsClient(HttpClient httpClient, IOptions<PayOsSetting> payOsSetting) : IPayOsClient
{
    private readonly PayOsSetting _payOsSetting = payOsSetting.Value;
    private PayOSClient? _client;

    /// <summary>Lazily builds the SDK client so misconfiguration surfaces as an app error code.</summary>
    private PayOSClient Client => _client ??= CreateClient();

    private PayOSClient CreateClient()
    {
        try
        {
            return new PayOSClient(
                new PayOSOptions
                {
                    ClientId = RealValue(_payOsSetting.ClientId),
                    ApiKey = RealValue(_payOsSetting.ApiKey),
                    ChecksumKey = RealValue(_payOsSetting.ChecksumKey),
                    PartnerCode = RealValue(_payOsSetting.PartnerCode),
                    BaseUrl = RealValue(_payOsSetting.BaseUrl) ?? "https://api-merchant.payos.vn",
                    HttpClient = httpClient,
                }
            );
        }
        catch (PayOsException ex)
        {
            throw new InternalServerException(
                ShopErrorCode.PayOsError,
                "PayOS credentials are not configured.",
                ex
            );
        }
    }

    /// <summary>Treats empty values and appsettings placeholders as unset (falls back to environment variables).</summary>
    private static string? RealValue(string value) =>
        string.IsNullOrWhiteSpace(value)
        || value.StartsWith("YOUR_", StringComparison.OrdinalIgnoreCase)
            ? null
            : value;

    /// <inheritdoc />
    public async Task<CreatePaymentLinkResponse> CreatePaymentLinkAsync(
        CreatePaymentLinkRequest request,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            return await Client.PaymentRequests.CreateAsync(
                request,
                new RequestOptions<CreatePaymentLinkRequest>
                {
                    CancellationToken = cancellationToken,
                }
            );
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new InternalServerException(ShopErrorCode.PayOsError, ex.Message, ex);
        }
    }

    /// <inheritdoc />
    public async Task<PaymentLink> GetPaymentLinkInformationAsync(
        long orderCode,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            return await Client.PaymentRequests.GetAsync(
                orderCode,
                new RequestOptions { CancellationToken = cancellationToken }
            );
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new InternalServerException(ShopErrorCode.PayOsError, ex.Message, ex);
        }
    }

    /// <inheritdoc />
    public async Task<WebhookData> VerifyWebhookAsync(Webhook webhook)
    {
        try
        {
            return await Client.Webhooks.VerifyAsync(webhook);
        }
        catch (WebhookException)
        {
            throw new UnauthorizedException(ShopErrorCode.InvalidWebhookSignature);
        }
    }

    /// <inheritdoc />
    public async Task ConfirmWebhookAsync(
        string webhookUrl,
        CancellationToken cancellationToken = default
    )
    {
        try
        {
            await Client.Webhooks.ConfirmAsync(
                webhookUrl,
                new RequestOptions<ConfirmWebhookRequest> { CancellationToken = cancellationToken }
            );
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new InternalServerException(ShopErrorCode.ConfirmWebhookFailed, ex.Message, ex);
        }
    }
}
