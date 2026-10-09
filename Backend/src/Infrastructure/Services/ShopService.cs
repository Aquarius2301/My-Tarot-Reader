using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MyTarotReader.Application.Common.Exceptions;
using MyTarotReader.Application.Common.Validators;
using MyTarotReader.Application.Constants.Errors;
using MyTarotReader.Application.Contracts.Common;
using MyTarotReader.Application.Contracts.Persistence;
using MyTarotReader.Application.Contracts.Services;
using MyTarotReader.Application.Settings;
using MyTarotReader.Domain.Entities;
using MyTarotReader.Domain.Enums;
using PayOS.Models.V2.PaymentRequests;

namespace MyTarotReader.Infrastructure.Services;

public class ShopService(
    IAppDbContext context,
    IWalletService walletService,
    IPayOsClient payOsClient,
    IValidator<CreatePaymentRequest> createPaymentValidator,
    IOptions<ShopSetting> shopSetting,
    IOptions<PayOsSetting> payOsSetting,
    ILogger<ShopService> logger
) : IShopService
{
    private readonly IAppDbContext _context = context;
    private readonly IWalletService _walletService = walletService;
    private readonly IPayOsClient _payOsClient = payOsClient;
    private readonly IValidator<CreatePaymentRequest> _createPaymentValidator =
        createPaymentValidator;
    private readonly ShopSetting _shopSetting = shopSetting.Value;
    private readonly PayOsSetting _payOsSetting = payOsSetting.Value;
    private readonly ILogger<ShopService> _logger = logger;

    /// <inheritdoc />
    public Task<GetPackagesResult> GetPackagesAsync(CancellationToken cancellationToken = default)
    {
        var packages = _shopSetting
            .Packages.Select(p => new GetPackagesItem(p.Code, p.RedCoins, p.PriceVnd))
            .ToList();

        return Task.FromResult(new GetPackagesResult(packages));
    }

    /// <inheritdoc />
    /// <remarks>
    /// A pending <see cref="PaymentOrder"/> is persisted first so the webhook can be matched
    /// by order code; if the PayOS link creation fails the order is marked failed.
    /// </remarks>
    public async Task<CreatePaymentResult> CreatePaymentAsync(
        Guid userId,
        CreatePaymentRequest request,
        CancellationToken cancellationToken = default
    )
    {
        ValidationHelper.ValidateOrThrow(_createPaymentValidator, request);

        var package =
            _shopSetting.Packages.FirstOrDefault(p =>
                string.Equals(p.Code, request.PackageCode, StringComparison.OrdinalIgnoreCase)
            ) ?? throw new BadRequestException(ShopErrorCode.InvalidPackage);

        var orderCode = GenerateOrderCode();
        var paymentOrder = new PaymentOrder
        {
            UserId = userId,
            OrderCode = orderCode,
            PackageCode = package.Code,
            AmountVnd = package.PriceVnd,
            RedCoins = package.RedCoins,
            Status = PaymentOrderStatus.Pending,
        };
        _context.PaymentOrders.Add(paymentOrder);
        await _context.SaveChangesAsync(cancellationToken);

        CreatePaymentLinkResponse response;
        try
        {
            response = await _payOsClient.CreatePaymentLinkAsync(
                new CreatePaymentLinkRequest
                {
                    OrderCode = orderCode,
                    Amount = package.PriceVnd,
                    Description = $"Mua {package.RedCoins} xu",
                    ReturnUrl = AppendOrderCode(_payOsSetting.ReturnUrl, orderCode),
                    CancelUrl = AppendOrderCode(_payOsSetting.CancelUrl, orderCode),
                    Items =
                    [
                        new PaymentLinkItem
                        {
                            Name = $"Goi {package.Code}",
                            Quantity = 1,
                            Price = package.PriceVnd,
                        },
                    ],
                },
                cancellationToken
            );
        }
        catch (InternalServerException ex)
        {
            paymentOrder.Status = PaymentOrderStatus.Failed;
            await _context.SaveChangesAsync(cancellationToken);
            throw new InternalServerException(ShopErrorCode.CreatePaymentFailed, ex.Message, ex);
        }

        paymentOrder.PayOsPaymentLinkId = string.IsNullOrEmpty(response.PaymentLinkId)
            ? null
            : response.PaymentLinkId;
        await _context.SaveChangesAsync(cancellationToken);

        return new CreatePaymentResult(
            paymentOrder.Id,
            orderCode,
            response.CheckoutUrl,
            response.QrCode,
            package.PriceVnd,
            package.RedCoins
        );
    }

    /// <inheritdoc />
    public async Task<GetOrderStatusResult> GetOrderStatusAsync(
        Guid userId,
        GetOrderStatusRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var order = await GetOwnedOrderAsync(userId, request.Id, cancellationToken);

        if (order.Status == PaymentOrderStatus.Pending)
        {
            await ReconcilePendingOrderAsync(order, cancellationToken);
            order = await GetOwnedOrderAsync(userId, request.Id, cancellationToken);
        }

        return new GetOrderStatusResult(
            order.Id,
            order.OrderCode,
            ToStatusString(order.Status),
            order.AmountVnd,
            order.RedCoins,
            order.PaidAt
        );
    }

    /// <inheritdoc />
    public async Task<HandleWebhookResult> HandleWebhookAsync(
        HandleWebhookRequest request,
        CancellationToken cancellationToken = default
    )
    {
        var data = await _payOsClient.VerifyWebhookAsync(request.Webhook);

        if (request.Webhook.Code != "00" || !request.Webhook.Success)
        {
            return new HandleWebhookResult(false);
        }

        var order = await _context
            .PaymentOrders.AsNoTracking()
            .Where(o => o.OrderCode == data.OrderCode)
            .Select(o => new PaymentOrder
            {
                Id = o.Id,
                UserId = o.UserId,
                OrderCode = o.OrderCode,
                AmountVnd = o.AmountVnd,
                RedCoins = o.RedCoins,
                Status = o.Status,
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (order is null)
        {
            // PayOS validation ping or an unknown order: acknowledge without processing.
            return new HandleWebhookResult(false);
        }

        if (order.Status == PaymentOrderStatus.Paid)
        {
            // Replay of an already-processed notification.
            return new HandleWebhookResult(false);
        }

        if (data.Amount != order.AmountVnd)
        {
            _logger.LogError(
                "PayOS webhook for order {OrderCode} reports {Amount} VND but {Expected} VND was expected; not crediting.",
                data.OrderCode,
                data.Amount,
                order.AmountVnd
            );
            return new HandleWebhookResult(false);
        }

        var processed = await FinalizePaidOrderAsync(order, data.Reference, cancellationToken);
        return new HandleWebhookResult(processed);
    }

    /// <summary>Loads one of the user's payment orders without tracking it.</summary>
    /// <exception cref="NotFoundException">Thrown when the order does not exist for the user.</exception>
    private async Task<PaymentOrder> GetOwnedOrderAsync(
        Guid userId,
        Guid orderId,
        CancellationToken cancellationToken
    ) =>
        await _context
            .PaymentOrders.AsNoTracking()
            .Where(o => o.Id == orderId && o.UserId == userId)
            .Select(o => new PaymentOrder
            {
                Id = o.Id,
                UserId = o.UserId,
                OrderCode = o.OrderCode,
                PackageCode = o.PackageCode,
                AmountVnd = o.AmountVnd,
                RedCoins = o.RedCoins,
                Status = o.Status,
                PaidAt = o.PaidAt,
            })
            .FirstOrDefaultAsync(cancellationToken)
        ?? throw new NotFoundException(ShopErrorCode.OrderNotFound);

    /// <summary>
    /// Asks PayOS for the real status of a pending order and finalizes or closes it accordingly.
    /// PayOS errors are logged and swallowed so polling degrades to the current status.
    /// </summary>
    private async Task ReconcilePendingOrderAsync(
        PaymentOrder order,
        CancellationToken cancellationToken
    )
    {
        PaymentLink link;
        try
        {
            link = await _payOsClient.GetPaymentLinkInformationAsync(
                order.OrderCode,
                cancellationToken
            );
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "Could not reconcile PayOS order {OrderCode}; keeping status pending.",
                order.OrderCode
            );
            return;
        }

        switch (link.Status)
        {
            case PaymentLinkStatus.Paid:
                if (link.AmountPaid < order.AmountVnd)
                {
                    _logger.LogError(
                        "PayOS order {OrderCode} paid {AmountPaid} VND but {AmountVnd} VND was expected; not crediting.",
                        order.OrderCode,
                        link.AmountPaid,
                        order.AmountVnd
                    );
                    return;
                }
                await FinalizePaidOrderAsync(
                    order,
                    link.Transactions?.FirstOrDefault()?.Reference,
                    cancellationToken
                );
                return;

            case PaymentLinkStatus.Cancelled:
                await ClaimOrderStatusAsync(order.Id, PaymentOrderStatus.Cancelled, cancellationToken);
                return;

            case PaymentLinkStatus.Expired:
                await ClaimOrderStatusAsync(order.Id, PaymentOrderStatus.Expired, cancellationToken);
                return;

            case PaymentLinkStatus.Failed:
                await ClaimOrderStatusAsync(order.Id, PaymentOrderStatus.Failed, cancellationToken);
                return;

            default:
                return;
        }
    }

    /// <summary>
    /// Marks a pending order as paid (guarded by a conditional update so concurrent
    /// webhook/polling deliveries credit the coins exactly once) and credits the red coins.
    /// </summary>
    /// <returns><c>true</c> when this call claimed the order and credited the coins.</returns>
    private async Task<bool> FinalizePaidOrderAsync(
        PaymentOrder order,
        string? reference,
        CancellationToken cancellationToken
    )
    {
        await using var transaction = await _context.Database.BeginTransactionAsync(
            cancellationToken
        );

        var claimed = await _context
            .PaymentOrders.Where(o => o.Id == order.Id && o.Status == PaymentOrderStatus.Pending)
            .ExecuteUpdateAsync(
                setters =>
                    setters
                        .SetProperty(o => o.Status, PaymentOrderStatus.Paid)
                        .SetProperty(o => o.PaidAt, DateTimeOffset.UtcNow)
                        .SetProperty(o => o.PayOsTransactionReference, reference),
                cancellationToken
            );

        if (claimed == 0)
        {
            return false;
        }

        await _walletService.AddCoinAsync(
            order.UserId,
            new AddCoinRequest(0, order.RedCoins, OrderType.TopUp),
            cancellationToken
        );

        await transaction.CommitAsync(cancellationToken);
        return true;
    }

    /// <summary>Closes a pending order with a terminal status, unless it was already updated.</summary>
    private async Task ClaimOrderStatusAsync(
        Guid orderId,
        PaymentOrderStatus status,
        CancellationToken cancellationToken
    ) =>
        await _context
            .PaymentOrders.Where(o => o.Id == orderId && o.Status == PaymentOrderStatus.Pending)
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(o => o.Status, status),
                cancellationToken
            );

    /// <summary>Generates a unique PayOS order code within the max-safe-number range.</summary>
    private static long GenerateOrderCode() =>
        DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() * 1000 + Random.Shared.Next(1000);

    /// <summary>Appends the order code to a return/cancel url as a query parameter.</summary>
    private static string AppendOrderCode(string url, long orderCode) =>
        string.IsNullOrEmpty(url)
            ? url
            : $"{url}{(url.Contains('?') ? '&' : '?')}orderCode={orderCode}";

    private static string ToStatusString(PaymentOrderStatus status) =>
        status.ToString().ToLowerInvariant();
}
