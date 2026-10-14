using MediatR;

namespace MyTarotReader.Application.Features.Shop;

public record GetOrderStatusQuery(Guid Id, Guid UserId) : IRequest<GetOrderStatusResult>;

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
