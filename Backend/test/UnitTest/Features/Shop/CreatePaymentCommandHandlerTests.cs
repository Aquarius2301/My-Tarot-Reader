using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using MyTarotReader.Application.Common.Exceptions;
using MyTarotReader.Application.Constants.Errors;
using MyTarotReader.Application.Features.Shop;
using MyTarotReader.Application.Services;
using MyTarotReader.Application.Settings;
using MyTarotReader.Domain.Entities;
using MyTarotReader.Domain.Enums;
using MyTarotReader.Infrastructure.Persistence;
using MyTarotReader.UnitTest.Common;
using PayOS.Models.V2.PaymentRequests;
using Xunit;

namespace MyTarotReader.UnitTest.Features.Shop;

/// <summary>
/// Unit tests for <see cref="CreatePaymentCommandHandler"/>. PayOS runs through a mock;
/// persistence runs against a real <see cref="AppDbContext"/> with the EF InMemory provider.
/// </summary>
public class CreatePaymentCommandHandlerTests
{
    private const int PackagePrice = 10000;
    private const int PackageRedCoins = 10;
    private const int PackageWhiteCoins = 2;
    private const string CheckoutUrl = "https://pay.example/checkout";
    private const string QrCode = "qr";

    #region Helpers

    private static (
        CreatePaymentCommandHandler Sut,
        AppDbContext Db,
        Mock<IPayOsClientService> PayOs
    ) CreateSut()
    {
        var db = TestDbContextFactory.Create();
        var payOs = new Mock<IPayOsClientService>();
        payOs
            .Setup(p =>
                p.CreatePaymentLinkAsync(
                    It.IsAny<CreatePaymentLinkRequest>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(
                new CreatePaymentLinkResponse
                {
                    PaymentLinkId = "link-1",
                    CheckoutUrl = CheckoutUrl,
                    QrCode = QrCode,
                }
            );

        var sut = new CreatePaymentCommandHandler(
            db,
            Options.Create(
                new PayOsSetting
                {
                    ReturnUrl = "https://example.com/success",
                    CancelUrl = "https://example.com/cancel",
                }
            ),
            payOs.Object
        );

        return (sut, db, payOs);
    }

    private static void SeedPackage(
        AppDbContext db,
        string code,
        int price = PackagePrice,
        bool isEnabled = true
    )
    {
        db.ShopPackages.Add(
            new ShopPackage
            {
                Code = code,
                RedCoins = PackageRedCoins,
                WhiteCoins = PackageWhiteCoins,
                OldPrice = price,
                Price = price,
                IsEnabled = isEnabled,
                CreatedAt = DateTimeOffset.UtcNow,
            }
        );
        db.SaveChanges();
    }

    private static void VerifyPayOsWithAmount(Mock<IPayOsClientService> payOs, long amount) =>
        payOs.Verify(
            p =>
                p.CreatePaymentLinkAsync(
                    It.Is<CreatePaymentLinkRequest>(r => r.Amount == amount),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once()
        );

    private static void VerifyPayOsNeverCalled(Mock<IPayOsClientService> payOs) =>
        payOs.Verify(
            p =>
                p.CreatePaymentLinkAsync(
                    It.IsAny<CreatePaymentLinkRequest>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Never()
        );

    #endregion

    [Fact]
    public async Task CreatePayment_ExistingPackage_PersistsOrderWithPackageCoins()
    {
        // Arrange
        var (sut, db, _) = CreateSut();
        SeedPackage(db, "pack-10");
        var userId = Guid.NewGuid();

        // Act
        var result = await sut.Handle(
            new CreatePaymentCommand(userId, "pack-10"),
            CancellationToken.None
        );

        // Assert
        var order = db.PaymentOrders.Should().ContainSingle().Subject;
        order.UserId.Should().Be(userId);
        order.PackageCode.Should().Be("pack-10");
        order.AmountVnd.Should().Be(PackagePrice);
        order.RedCoins.Should().Be(PackageRedCoins);
        order.WhiteCoins.Should().Be(PackageWhiteCoins);
        order.Status.Should().Be(PaymentOrderStatus.Pending);
        result.AmountVnd.Should().Be(PackagePrice);
        result.RedCoins.Should().Be(PackageRedCoins);
        result.WhiteCoins.Should().Be(PackageWhiteCoins);
        result.OrderId.Should().Be(order.Id);
        result.OrderCode.Should().Be(order.OrderCode);
        result.CheckoutUrl.Should().Be(CheckoutUrl);
        result.QrCode.Should().Be(QrCode);
    }

    [Fact]
    public async Task CreatePayment_ExistingPackage_CallsPayOsWithPackagePrice()
    {
        // Arrange
        var (sut, db, payOs) = CreateSut();
        SeedPackage(db, "pack-10");

        // Act
        await sut.Handle(new CreatePaymentCommand(Guid.NewGuid(), "pack-10"), CancellationToken.None);

        // Assert
        VerifyPayOsWithAmount(payOs, PackagePrice);
    }

    [Fact]
    public async Task CreatePayment_UnknownPackage_ThrowsBadRequestInvalidPackage()
    {
        // Arrange
        var (sut, db, payOs) = CreateSut();
        SeedPackage(db, "pack-10");

        // Act
        var act = () =>
            sut.Handle(new CreatePaymentCommand(Guid.NewGuid(), "pack-999"), CancellationToken.None);

        // Assert
        var ex = await act.Should().ThrowAsync<BadRequestException>();
        ex.Which.ErrorCode.Should().Be(ShopErrorCode.InvalidPackage);
        db.PaymentOrders.Should().BeEmpty();
        VerifyPayOsNeverCalled(payOs);
    }

    [Fact]
    public async Task CreatePayment_DisabledPackage_ThrowsBadRequestInvalidPackage()
    {
        // Arrange
        var (sut, db, payOs) = CreateSut();
        SeedPackage(db, "pack-10", isEnabled: false);

        // Act
        var act = () =>
            sut.Handle(new CreatePaymentCommand(Guid.NewGuid(), "pack-10"), CancellationToken.None);

        // Assert
        var ex = await act.Should().ThrowAsync<BadRequestException>();
        ex.Which.ErrorCode.Should().Be(ShopErrorCode.InvalidPackage);
        db.PaymentOrders.Should().BeEmpty();
        VerifyPayOsNeverCalled(payOs);
    }

    [Fact]
    public async Task CreatePayment_PackageCodeCasingIsIgnored_MatchesPackage()
    {
        // Arrange
        var (sut, db, _) = CreateSut();
        SeedPackage(db, "pack-10");

        // Act
        var result = await sut.Handle(
            new CreatePaymentCommand(Guid.NewGuid(), "  PACK-10  "),
            CancellationToken.None
        );

        // Assert
        result.AmountVnd.Should().Be(PackagePrice);
        db.PaymentOrders.Should().ContainSingle();
    }
}
