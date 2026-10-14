using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyTarotReader.Application.Common.Exceptions;
using MyTarotReader.Application.Constants.Errors;
using MyTarotReader.Application.Persistence;
using MyTarotReader.Application.Services;
using MyTarotReader.Domain.Entities;
using MyTarotReader.Domain.Enums;
using PayOS.Models.V2.PaymentRequests;

namespace MyTarotReader.Application.Features.Shop;

public class GetOrderStatusQueryHandler(
    IAppDbContext context,
    ILogger<GetOrderStatusQueryHandler> logger,
    IWalletService walletService,
    IPayOsClientService payOsClientService
) : IRequestHandler<GetOrderStatusQuery, GetOrderStatusResult>
{
    public async Task<GetOrderStatusResult> Handle(
        GetOrderStatusQuery request,
        CancellationToken cancellationToken = default
    )
    {
        var order = await GetOwnedOrderAsync(request.UserId, request.Id, cancellationToken);

        if (order.Status == PaymentOrderStatus.Pending)
        {
            await ReconcilePendingOrderAsync(order, cancellationToken);
            order = await GetOwnedOrderAsync(request.UserId, request.Id, cancellationToken);
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

    /// <summary>Loads one of the user's payment orders without tracking it.</summary>
    /// <exception cref="NotFoundException">Thrown when the order does not exist for the user.</exception>
    private async Task<PaymentOrder> GetOwnedOrderAsync(
        Guid userId,
        Guid orderId,
        CancellationToken cancellationToken
    ) =>
        await context
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
            link = await payOsClientService.GetPaymentLinkInformationAsync(
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
            logger.LogWarning(
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
                    logger.LogError(
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
                await ClaimOrderStatusAsync(
                    order.Id,
                    PaymentOrderStatus.Cancelled,
                    cancellationToken
                );
                return;

            case PaymentLinkStatus.Expired:
                await ClaimOrderStatusAsync(
                    order.Id,
                    PaymentOrderStatus.Expired,
                    cancellationToken
                );
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
        await using var transaction = await context.Database.BeginTransactionAsync(
            cancellationToken
        );

        var claimed = await context
            .PaymentOrders.Where(o => o.Id == order.Id && o.Status == PaymentOrderStatus.Pending)
            .ExecuteUpdateAsync(
                setters =>
                    setters
                        .SetProperty(o => o.Status, PaymentOrderStatus.Paid)
                        .SetProperty(o => o.PaidAt, DateTimeOffset.UtcNow)
                        .SetProperty(o => o.PayOsTransactionReference, reference),
                cancellationToken: cancellationToken
            );

        if (claimed == 0)
        {
            return false;
        }

        await walletService.AddCoinsAsync(
            order.UserId,
            order.RedCoins,
            0,
            OrderType.TopUp,
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
        await context
            .PaymentOrders.Where(o => o.Id == orderId && o.Status == PaymentOrderStatus.Pending)
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(o => o.Status, status),
                cancellationToken
            );

    private static string ToStatusString(PaymentOrderStatus status) =>
        status.ToString().ToLowerInvariant();
}
