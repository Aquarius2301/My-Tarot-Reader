using MediatR;

namespace MyTarotReader.Application.Features.Shop.Commands.CreatePayment;

public record CreatePaymentCommand(Guid UserId, string PackageCode) : IRequest<CreatePaymentResult>;

public record CreatePaymentResult(
    Guid OrderId,
    long OrderCode,
    string CheckoutUrl,
    string QrCode,
    int AmountVnd,
    int RedCoins
);
