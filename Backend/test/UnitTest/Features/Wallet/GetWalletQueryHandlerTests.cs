using FluentAssertions;
using MyTarotReader.Application.Common.Exceptions;
using MyTarotReader.Application.Constants.Errors;
using MyTarotReader.Application.Features.Wallet;
using MyTarotReader.Application.Features.Wallet.Queries;
using MyTarotReader.Domain.Entities;
using MyTarotReader.Infrastructure.Persistence;
using MyTarotReader.UnitTest.Common;
using Xunit;
using WalletEntity = MyTarotReader.Domain.Entities.Wallet;

namespace MyTarotReader.UnitTest.Features.Wallet;

public class GetWalletQueryHandlerTests
{
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
    public async Task Handle_WithMixedBatches_ReturnsBalanceAndActiveBatchesOrderedByExpiry()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var db = TestDbContextFactory.Create();
        SeedWallet(
            db,
            userId,
            redCoin: 4,
            new WhiteCoinBatch
            {
                Amount = 3,
                RemainingAmount = 3,
                ExpiredAt = DateTimeOffset.UtcNow.AddDays(5),
            },
            new WhiteCoinBatch
            {
                Amount = 5,
                RemainingAmount = 5,
                ExpiredAt = DateTimeOffset.UtcNow.AddDays(1),
            },
            new WhiteCoinBatch
            {
                Amount = 10,
                RemainingAmount = 10,
                ExpiredAt = DateTimeOffset.UtcNow.AddDays(-1),
            },
            new WhiteCoinBatch
            {
                Amount = 2,
                RemainingAmount = 0,
                ExpiredAt = DateTimeOffset.UtcNow.AddDays(10),
            }
        );
        var sut = new GetWalletQueryHandler(db);

        // Act
        var result = await sut.Handle(new GetWalletQuery(userId), CancellationToken.None);

        // Assert
        result.RedCoin.Should().Be(4);
        result.WhiteCoin.Should().Be(8);
        result.WhiteCoinBatches.Should().HaveCount(2);
        result.WhiteCoinBatches.Select(b => b.RemainingAmount).Should().ContainInOrder(5, 3);
        result.WhiteCoinBatches.Should().BeInAscendingOrder(b => b.ExpiredAt);
    }

    [Fact]
    public async Task Handle_WalletNotFound_ThrowsNotFound()
    {
        // Arrange
        var db = TestDbContextFactory.Create();
        var sut = new GetWalletQueryHandler(db);

        // Act
        var act = () => sut.Handle(new GetWalletQuery(Guid.NewGuid()), CancellationToken.None);

        // Assert
        var ex = await act.Should().ThrowAsync<NotFoundException>();
        ex.Which.ErrorCode.Should().Be(WalletErrorCode.WalletNotFound);
    }
}
