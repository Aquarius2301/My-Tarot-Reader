using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MyTarotReader.Application.Common.Exceptions;
using MyTarotReader.Application.Constants.Errors;
using MyTarotReader.Application.Features.Wallet;
using MyTarotReader.Application.Settings;
using MyTarotReader.Domain.Entities;
using MyTarotReader.Domain.Enums;
using MyTarotReader.Infrastructure.Persistence;
using MyTarotReader.UnitTest.Common;
using Xunit;
using WalletEntity = MyTarotReader.Domain.Entities.Wallet;

namespace MyTarotReader.UnitTest.Features.Wallet;

public class ConvertRedToWhiteCommandHandlerTests
{
    private static ConvertRedToWhiteCommandHandler CreateSut(AppDbContext db) =>
        new(db, Options.Create(new WalletSetting { ExpireDays = 30 }));

    private static WalletEntity SeedWallet(
        AppDbContext db,
        Guid userId,
        int redCoin,
        params WhiteCoinBatch[] batches
    )
    {
        var wallet = new WalletEntity { UserId = userId, RedCoin = redCoin };
        foreach (var batch in batches)
        {
            batch.WalletId = wallet.Id;
            wallet.WhiteCoinBatches.Add(batch);
        }

        db.Wallets.Add(wallet);
        db.SaveChanges();
        return wallet;
    }

    [Fact]
    public async Task Handle_ValidRequest_DeductsRedGrantsDoubleWhiteAndRecordsOrder()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var db = TestDbContextFactory.Create();
        var wallet = SeedWallet(db, userId, redCoin: 5);
        var sut = CreateSut(db);

        // Act
        await sut.Handle(new ConvertRedToWhiteCommand(userId, 2), CancellationToken.None);

        // Assert
        wallet.RedCoin.Should().Be(3);

        var batch = db.WhiteCoinBatches.Should().ContainSingle().Subject;
        batch.Amount.Should().Be(4);
        batch.RemainingAmount.Should().Be(4);
        batch.WalletId.Should().Be(wallet.Id);
        batch.ExpiredAt.Should().BeCloseTo(DateTimeOffset.UtcNow.AddDays(30), TimeSpan.FromMinutes(5));

        var order = db.Orders.Should().ContainSingle().Subject;
        order.UserId.Should().Be(userId);
        order.Type.Should().Be(OrderType.RedToWhite);
        order.RedCoins.Should().Be(0);
        order.WhiteCoins.Should().Be(4);
    }

    [Fact]
    public async Task Handle_ExactBalance_LeavesZeroAndRecordsOrder()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var db = TestDbContextFactory.Create();
        var wallet = SeedWallet(db, userId, redCoin: 3);
        var sut = CreateSut(db);

        // Act
        await sut.Handle(new ConvertRedToWhiteCommand(userId, 3), CancellationToken.None);

        // Assert
        wallet.RedCoin.Should().Be(0);
        db.WhiteCoinBatches.Should().ContainSingle().Which.Amount.Should().Be(6);
    }

    [Fact]
    public async Task Handle_WithExistingBatch_AddsConvertedBatchOnTop()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var db = TestDbContextFactory.Create();
        var existing = new WhiteCoinBatch
        {
            Amount = 3,
            RemainingAmount = 3,
            ExpiredAt = DateTimeOffset.UtcNow.AddDays(10),
        };
        SeedWallet(db, userId, redCoin: 1, existing);
        var sut = CreateSut(db);

        // Act
        await sut.Handle(new ConvertRedToWhiteCommand(userId, 1), CancellationToken.None);

        // Assert
        db.WhiteCoinBatches.Should().HaveCount(2);
        db.WhiteCoinBatches.Sum(b => b.RemainingAmount).Should().Be(5);
    }

    [Fact]
    public async Task Handle_InsufficientRed_ThrowsBadRequestAndNoChanges()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var db = TestDbContextFactory.Create();
        var wallet = SeedWallet(db, userId, redCoin: 1);
        var sut = CreateSut(db);

        // Act
        var act = () => sut.Handle(new ConvertRedToWhiteCommand(userId, 3), CancellationToken.None);

        // Assert
        var ex = await act.Should().ThrowAsync<BadRequestException>();
        ex.Which.ErrorCode.Should().Be(WalletErrorCode.InsufficientRedCoins);
        wallet.RedCoin.Should().Be(1);
        db.WhiteCoinBatches.Should().BeEmpty();
        db.Orders.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WalletNotFound_ThrowsNotFound()
    {
        // Arrange
        var db = TestDbContextFactory.Create();
        var sut = CreateSut(db);

        // Act
        var act = () => sut.Handle(new ConvertRedToWhiteCommand(Guid.NewGuid(), 1), CancellationToken.None);

        // Assert
        var ex = await act.Should().ThrowAsync<NotFoundException>();
        ex.Which.ErrorCode.Should().Be(WalletErrorCode.WalletNotFound);
    }
}
