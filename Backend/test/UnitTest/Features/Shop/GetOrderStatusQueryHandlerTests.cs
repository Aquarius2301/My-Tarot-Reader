using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using MyTarotReader.Application.Features.Shop;
using MyTarotReader.Application.Services;
using MyTarotReader.Domain.Entities;
using MyTarotReader.Domain.Enums;
using MyTarotReader.Infrastructure.Persistence;
using MyTarotReader.UnitTest.Common;
using PayOS.Models.V2.PaymentRequests;
using Xunit;

namespace MyTarotReader.UnitTest.Features.Shop;

/// <summary>
/// Unit tests for <see cref="GetOrderStatusQueryHandler"/>. Persistence runs against a
/// real <see cref="AppDbContext"/> with the EF InMemory provider; PayOS is mocked.
/// </summary>
public class GetOrderStatusQueryHandlerTests
{
    private const long OrderCode = 123456;
    private const int AmountVnd = 10000;
    private const int RedCoins = 10;
    private const int WhiteCoins = 2;
    private const string CheckoutUrl = "https://pay.example/checkout";
    private const string QrCode = "qr";

    private static PaymentOrder SeedOrder(
        AppDbContext db,
        Guid userId,
        PaymentOrderStatus status
    )
    {
        var order = new PaymentOrder
        {
            UserId = userId,
            OrderCode = OrderCode,
            PackageCode = "pack-10",
            AmountVnd = AmountVnd,
            RedCoins = RedCoins,
            WhiteCoins = WhiteCoins,
            Status = status,
            CheckoutUrl = CheckoutUrl,
            QrCode = QrCode,
        };
        db.PaymentOrders.Add(order);
        db.SaveChanges();
        return order;
    }

    private static (GetOrderStatusQueryHandler Sut, Mock<IPayOsClientService> PayOs) CreateSut(
        AppDbContext db
    )
    {
        var payOs = new Mock<IPayOsClientService>();
        payOs
            .Setup(p =>
                p.GetPaymentLinkInformationAsync(
                    It.IsAny<long>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(new PaymentLink { OrderCode = OrderCode, Status = PaymentLinkStatus.Pending });

        var sut = new GetOrderStatusQueryHandler(
            db,
            Mock.Of<ILogger<GetOrderStatusQueryHandler>>(),
            Mock.Of<IWalletService>(),
            payOs.Object
        );

        return (sut, payOs);
    }

    [Fact]
    public async Task Handle_PendingOrder_ReturnsCheckoutUrlAndQrCode()
    {
        // Arrange
        using var db = TestDbContextFactory.Create();
        var (sut, _) = CreateSut(db);
        var userId = Guid.NewGuid();
        var order = SeedOrder(db, userId, PaymentOrderStatus.Pending);

        // Act
        var result = await sut.Handle(
            new GetOrderStatusQuery(order.Id, userId),
            CancellationToken.None
        );

        // Assert
        result.Status.Should().Be("pending");
        result.CheckoutUrl.Should().Be(CheckoutUrl);
        result.QrCode.Should().Be(QrCode);
    }

    [Fact]
    public async Task Handle_SettledOrder_HidesCheckoutUrlAndQrCode()
    {
        // Arrange
        using var db = TestDbContextFactory.Create();
        var (sut, payOs) = CreateSut(db);
        var userId = Guid.NewGuid();
        var order = SeedOrder(db, userId, PaymentOrderStatus.Paid);

        // Act
        var result = await sut.Handle(
            new GetOrderStatusQuery(order.Id, userId),
            CancellationToken.None
        );

        // Assert
        result.Status.Should().Be("paid");
        result.CheckoutUrl.Should().BeNull();
        result.QrCode.Should().BeNull();
        payOs.Verify(
            p =>
                p.GetPaymentLinkInformationAsync(
                    It.IsAny<long>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Never()
        );
    }
}
