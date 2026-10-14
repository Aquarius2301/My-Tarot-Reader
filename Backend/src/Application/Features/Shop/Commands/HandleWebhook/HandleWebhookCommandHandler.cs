using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyTarotReader.Application.Persistence;
using MyTarotReader.Application.Services;
using MyTarotReader.Domain.Entities;
using MyTarotReader.Domain.Enums;

namespace MyTarotReader.Application.Features.Shop;

public class HandleWebhookCommandHandler(
    IAppDbContext context,
    IPayOsClientService payOsClientService,
    IWalletService walletService,
    ILogger<HandleWebhookCommandHandler> logger
) : IRequestHandler<HandleWebhookCommand, HandleWebhookResult>
{
    public async Task<HandleWebhookResult> Handle(
        HandleWebhookCommand request,
        CancellationToken cancellationToken = default
    )
    {
        var data = await payOsClientService.VerifyWebhookAsync(request.Webhook);

        if (request.Webhook.Code != "00" || !request.Webhook.Success)
        {
            return new HandleWebhookResult(false);
        }

        var order = await context
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
            logger.LogError(
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
}
