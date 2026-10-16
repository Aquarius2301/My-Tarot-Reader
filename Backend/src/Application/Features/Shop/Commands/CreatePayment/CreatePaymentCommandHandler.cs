using System;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MyTarotReader.Application.Common.Exceptions;
using MyTarotReader.Application.Constants.Errors;
using MyTarotReader.Application.Persistence;
using MyTarotReader.Application.Services;
using MyTarotReader.Application.Settings;
using MyTarotReader.Domain.Entities;
using MyTarotReader.Domain.Enums;
using PayOS.Models.V2.PaymentRequests;

namespace MyTarotReader.Application.Features.Shop;

public class CreatePaymentCommandHandler(
    IAppDbContext context,
    IOptions<PayOsSetting> payOsSetting,
    IPayOsClientService payOsClientService
) : IRequestHandler<CreatePaymentCommand, CreatePaymentResult>
{
    private readonly PayOsSetting _payOsSetting = payOsSetting.Value;

    public async Task<CreatePaymentResult> Handle(
        CreatePaymentCommand request,
        CancellationToken cancellationToken = default
    )
    {
        var normalizedCode = request.PackageCode.Trim();
        var package =
            await context
                .ShopPackages.AsNoTracking()
                .FirstOrDefaultAsync(
                    p => p.IsEnabled && p.Code.ToLower() == normalizedCode.ToLower(),
                    cancellationToken
                )
            ?? throw new BadRequestException(ShopErrorCode.InvalidPackage);

        var orderCode = GenerateOrderCode();
        var paymentOrder = new PaymentOrder
        {
            UserId = request.UserId,
            OrderCode = orderCode,
            PackageCode = package.Code,
            AmountVnd = package.Price,
            RedCoins = package.RedCoins,
            WhiteCoins = package.WhiteCoins,
            Status = PaymentOrderStatus.Pending,
        };
        context.PaymentOrders.Add(paymentOrder);
        await context.SaveChangesAsync(cancellationToken);

        CreatePaymentLinkResponse response;
        try
        {
            response = await payOsClientService.CreatePaymentLinkAsync(
                new CreatePaymentLinkRequest
                {
                    OrderCode = orderCode,
                    Amount = package.Price,
                    Description = $"Mua {package.RedCoins} xu",
                    ReturnUrl = AppendOrderCode(_payOsSetting.ReturnUrl, orderCode),
                    CancelUrl = AppendOrderCode(_payOsSetting.CancelUrl, orderCode),
                    Items =
                    [
                        new PaymentLinkItem
                        {
                            Name = $"Goi {package.Code}",
                            Quantity = 1,
                            Price = package.Price,
                        },
                    ],
                },
                cancellationToken
            );
        }
        catch (InternalServerException ex)
        {
            paymentOrder.Status = PaymentOrderStatus.Failed;
            await context.SaveChangesAsync(cancellationToken);
            throw new InternalServerException(ShopErrorCode.CreatePaymentFailed, ex.Message, ex);
        }

        paymentOrder.PayOsPaymentLinkId = string.IsNullOrEmpty(response.PaymentLinkId)
            ? null
            : response.PaymentLinkId;
        await context.SaveChangesAsync(cancellationToken);

        return new CreatePaymentResult(
            paymentOrder.Id,
            orderCode,
            response.CheckoutUrl,
            response.QrCode,
            package.Price,
            package.RedCoins,
            package.WhiteCoins
        );
    }

    /// <summary>Generates a unique PayOS order code within the max-safe-number range.</summary>
    private static long GenerateOrderCode() =>
        DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() * 1000 + Random.Shared.Next(1000);

    /// <summary>Appends the order code to a return/cancel url as a query parameter.</summary>
    private static string AppendOrderCode(string url, long orderCode) =>
        string.IsNullOrEmpty(url)
            ? url
            : $"{url}{(url.Contains('?') ? '&' : '?')}orderCode={orderCode}";
}
