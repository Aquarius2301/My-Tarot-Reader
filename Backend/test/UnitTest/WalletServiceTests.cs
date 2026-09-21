using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MyTarotReader.Application.Common.Exceptions;
using MyTarotReader.Application.Common.Validators;
using MyTarotReader.Application.Constants.Errors;
using MyTarotReader.Application.Contracts.Services;
using MyTarotReader.Application.Settings;
using MyTarotReader.Domain.Entities;
using MyTarotReader.Domain.Enums;
using MyTarotReader.Infrastructure.Persistence;
using MyTarotReader.Infrastructure.Services;
using Xunit;

namespace MyTarotReader.UnitTest;

/// <summary>
/// Unit tests for <see cref="WalletService"/>, running against a real
/// <see cref="AppDbContext"/> with the EF Core InMemory provider. The validator is real
/// so that the full validation flow is exercised for <see cref="WalletService.AddCoinAsync"/>.
/// </summary>
public class WalletServiceTests
{
    private static readonly WalletSetting DefaultWallet = new()
    {
        InitialWhiteCoins = 5,
        ExpireDays = 30,
    };

    #region Helpers

    private static AppDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .EnableServiceProviderCaching(false)
            .Options;
        return new AppDbContext(options);
    }

    private static WalletService CreateService(AppDbContext db) =>
        new(
            db,
            Options.Create(DefaultWallet),
            new AddCoinRequestValidator(),
            new DeductCoinRequestValidator()
        );

    private static async Task<(User User, Wallet Wallet)> SeedUserAsync(
        AppDbContext db,
        int redCoin = 0,
        IReadOnlyList<WhiteCoinBatch>? batches = null
    )
    {
        var user = new User
        {
            FullName = "Jane",
            Email = "jane@example.com",
            Picture = "http://pic",
            ProviderKey = "google-1",
            Role = UserRole.Registered,
        };
        var wallet = new Wallet
        {
            UserId = user.Id,
            RedCoin = redCoin,
            UpdatedAt = DateTimeOffset.UtcNow,
            WhiteCoinBatches = batches is null ? [] : [.. batches],
        };
        db.Users.Add(user);
        db.Wallets.Add(wallet);
        await db.SaveChangesAsync();
        return (user, wallet);
    }

    #endregion

    #region AddCoinAsync

    /// <summary>
    /// Adding both white and red coins creates a <see cref="WhiteCoinBatch"/> whose
    /// Amount/RemainingAmount equal the requested white coins and whose ExpiredAt is
    /// <c>now + WalletSetting.ExpireDays</c>, increases the wallet's RedCoin, and records
    /// a single <see cref="Order"/> (amount = white + red) with one OrderDetail per coin type.
    /// </summary>
    [Fact]
    public async Task AddCoin_WhiteAndRed_CreatesBatchIncreasesRedAndRecordsOrder()
    {
        var db = CreateInMemoryContext();
        var service = CreateService(db);
        var (user, _) = await SeedUserAsync(db);
        var before = DateTimeOffset.UtcNow;

        var result = await service.AddCoinAsync(
            user.Id,
            new AddCoinRequest(2, 3, OrderType.TopUp)
        );

        var after = DateTimeOffset.UtcNow;

        var wallet = db.Wallets.Include(w => w.WhiteCoinBatches).Single();
        wallet.RedCoin.Should().Be(3);

        var batch = Assert.Single(wallet.WhiteCoinBatches);
        batch.Amount.Should().Be(2);
        batch.RemainingAmount.Should().Be(2);
        batch
            .ExpiredAt.Should()
            .BeOnOrAfter(before.AddDays(DefaultWallet.ExpireDays))
            .And.BeOnOrBefore(after.AddDays(DefaultWallet.ExpireDays));

        var order = db.Orders.Include(o => o.OrderDetails).Single();
        order.UserId.Should().Be(user.Id);
        order.Amount.Should().Be(5);
        order.Type.Should().Be(OrderType.TopUp);
        order.Description.Should().StartWith("Top-up coins");

        order.OrderDetails.Should().HaveCount(2);
        order
            .OrderDetails.Single(d => d.WhiteCoinBatchId == batch.Id)
            .Amount.Should()
            .Be(2);
        order.OrderDetails.Single(d => d.WhiteCoinBatchId is null).Amount.Should().Be(3);

        result.RedCoin.Should().Be(3);
        result.WhiteCoin.Should().Be(2);
    }

    /// <summary>
    /// A white-only top-up creates exactly one batch and one OrderDetail linked to that
    /// batch; RedCoin stays untouched.
    /// </summary>
    [Fact]
    public async Task AddCoin_WhiteOnly_CreatesNoRedDetail()
    {
        var db = CreateInMemoryContext();
        var service = CreateService(db);
        var (user, _) = await SeedUserAsync(db);

        var result = await service.AddCoinAsync(
            user.Id,
            new AddCoinRequest(5, 0, OrderType.TopUp)
        );

        var wallet = db.Wallets.Include(w => w.WhiteCoinBatches).Single();
        wallet.RedCoin.Should().Be(0);
        var batch = Assert.Single(wallet.WhiteCoinBatches);

        var order = db.Orders.Include(o => o.OrderDetails).Single();
        order.Amount.Should().Be(5);
        var detail = Assert.Single(order.OrderDetails);
        detail.WhiteCoinBatchId.Should().Be(batch.Id);
        detail.Amount.Should().Be(5);

        result.RedCoin.Should().Be(0);
        result.WhiteCoin.Should().Be(5);
    }

    /// <summary>
    /// A red-only top-up only bumps the wallet balance; no batch is created and the single
    /// OrderDetail carries no batch reference.
    /// </summary>
    [Fact]
    public async Task AddCoin_RedOnly_IncreasesRedWithoutBatch()
    {
        var db = CreateInMemoryContext();
        var service = CreateService(db);
        var (user, _) = await SeedUserAsync(db);

        var result = await service.AddCoinAsync(
            user.Id,
            new AddCoinRequest(0, 4, OrderType.TopUp)
        );

        var wallet = db.Wallets.Include(w => w.WhiteCoinBatches).Single();
        wallet.RedCoin.Should().Be(4);
        wallet.WhiteCoinBatches.Should().BeEmpty();

        var order = db.Orders.Include(o => o.OrderDetails).Single();
        order.Amount.Should().Be(4);
        var detail = Assert.Single(order.OrderDetails);
        detail.WhiteCoinBatchId.Should().BeNull();
        detail.Amount.Should().Be(4);

        result.RedCoin.Should().Be(4);
        result.WhiteCoin.Should().Be(0);
    }

    /// <summary>
    /// The returned <c>WhiteCoin</c> is the sum of RemainingAmount across active batches
    /// only: expired or fully-depleted batches are excluded.
    /// </summary>
    [Fact]
    public async Task AddCoin_WhiteCoinResult_CountsOnlyActiveBatches()
    {
        var db = CreateInMemoryContext();
        var service = CreateService(db);
        var (user, _) = await SeedUserAsync(
            db,
            redCoin: 2,
            batches:
            [
                new()
                {
                    Amount = 3,
                    RemainingAmount = 3,
                    ExpiredAt = DateTimeOffset.UtcNow.AddDays(10),
                },
                new()
                {
                    Amount = 5,
                    RemainingAmount = 5,
                    ExpiredAt = DateTimeOffset.UtcNow.AddDays(-1),
                },
                new()
                {
                    Amount = 4,
                    RemainingAmount = 0,
                    ExpiredAt = DateTimeOffset.UtcNow.AddDays(10),
                },
            ]
        );

        var result = await service.AddCoinAsync(
            user.Id,
            new AddCoinRequest(1, 0, OrderType.TopUp)
        );

        result.RedCoin.Should().Be(2);
        result.WhiteCoin.Should().Be(4); // 3 (active) + 1 (new)
    }

    /// <summary>
    /// A top-up for a user without a wallet throws <see cref="NotFoundException"/> with the
    /// <c>walletNotFound</c> code and creates no order.
    /// </summary>
    [Fact]
    public async Task AddCoin_WalletNotFound_ThrowsNotFound()
    {
        var db = CreateInMemoryContext();
        var service = CreateService(db);

        var act = async () =>
            await service.AddCoinAsync(
                Guid.NewGuid(),
                new AddCoinRequest(1, 1, OrderType.TopUp)
            );

        await act.Should()
            .ThrowAsync<NotFoundException>()
            .Where(e => e.ErrorCode == WalletErrorCode.WalletNotFound);
        db.Orders.Should().BeEmpty();
        db.WhiteCoinBatches.Should().BeEmpty();
    }

    /// <summary>
    /// Negative amounts or a zero-total request are rejected before any persistence:
    /// <see cref="BadRequestException"/> with the <c>invalidAmount</c> code.
    /// </summary>
    [Fact]
    public async Task AddCoin_InvalidAmount_ThrowsBadRequest()
    {
        var db = CreateInMemoryContext();
        var service = CreateService(db);
        var (user, _) = await SeedUserAsync(db);

        var requests = new[]
        {
            new AddCoinRequest(-1, 0, OrderType.TopUp),
            new AddCoinRequest(0, -1, OrderType.TopUp),
            new AddCoinRequest(0, 0, OrderType.TopUp),
        };

        foreach (var request in requests)
        {
            var act = async () => await service.AddCoinAsync(user.Id, request);

            await act.Should()
                .ThrowAsync<BadRequestException>()
                .Where(e => e.ErrorCode == WalletErrorCode.InvalidAmount);
        }

        db.Orders.Should().BeEmpty();
        db.WhiteCoinBatches.Should().BeEmpty();
    }

    #endregion

    #region GetBalanceAsync

    /// <summary>
    /// The balance sums RemainingAmount of active (non-expired, non-depleted) batches for
    /// white coins and returns the wallet's RedCoin.
    /// </summary>
    [Fact]
    public async Task GetBalance_WithActiveAndExpiredBatches_ReturnsWhiteAndRed()
    {
        var db = CreateInMemoryContext();
        var service = CreateService(db);
        var (user, _) = await SeedUserAsync(
            db,
            redCoin: 4,
            batches:
            [
                new()
                {
                    Amount = 3,
                    RemainingAmount = 3,
                    ExpiredAt = DateTimeOffset.UtcNow.AddDays(10),
                },
                new()
                {
                    Amount = 5,
                    RemainingAmount = 5,
                    ExpiredAt = DateTimeOffset.UtcNow.AddDays(-1),
                },
                new()
                {
                    Amount = 4,
                    RemainingAmount = 0,
                    ExpiredAt = DateTimeOffset.UtcNow.AddDays(10),
                },
            ]
        );

        var result = await service.GetBalanceAsync(user.Id);

        result.WhiteCoin.Should().Be(3);
        result.RedCoin.Should().Be(4);
    }

    /// <summary>
    /// A user without a wallet throws NotFoundException with the walletNotFound code.
    /// </summary>
    [Fact]
    public async Task GetBalance_WalletNotFound_ThrowsNotFound()
    {
        var db = CreateInMemoryContext();
        var service = CreateService(db);

        var act = async () => await service.GetBalanceAsync(Guid.NewGuid());

        await act.Should()
            .ThrowAsync<NotFoundException>()
            .Where(e => e.ErrorCode == WalletErrorCode.WalletNotFound);
    }

    #endregion

    #region DeductCoinAsync

    /// <summary>
    /// Deducting from a single batch reduces its RemainingAmount, records an order of the
    /// requested type with an OrderDetail linked to the spent batch, and returns the
    /// remaining white balance.
    /// </summary>
    [Fact]
    public async Task DeductCoin_SingleBatch_DeductsAndRecordsOrder()
    {
        var db = CreateInMemoryContext();
        var service = CreateService(db);
        var (user, _) = await SeedUserAsync(
            db,
            batches:
            [
                new()
                {
                    Amount = 5,
                    RemainingAmount = 5,
                    ExpiredAt = DateTimeOffset.UtcNow.AddDays(10),
                },
            ]
        );

        var result = await service.DeductCoinAsync(
            user.Id,
            new DeductCoinRequest(2, OrderType.AITarot)
        );

        var wallet = db.Wallets.Include(w => w.WhiteCoinBatches).Single();
        var batch = Assert.Single(wallet.WhiteCoinBatches);
        batch.RemainingAmount.Should().Be(3);
        wallet.RedCoin.Should().Be(0);

        var order = db.Orders.Include(o => o.OrderDetails).Single();
        order.UserId.Should().Be(user.Id);
        order.Amount.Should().Be(2);
        order.Type.Should().Be(OrderType.AITarot);
        order.Description.Should().StartWith("Spend coins");
        var detail = Assert.Single(order.OrderDetails);
        detail.WhiteCoinBatchId.Should().Be(batch.Id);
        detail.Amount.Should().Be(2);

        result.WhiteCoin.Should().Be(3);
    }

    /// <summary>
    /// When the deduction spans several batches, the oldest active batch (by CreatedAt) is
    /// consumed first and a separate OrderDetail is recorded per batch.
    /// </summary>
    [Fact]
    public async Task DeductCoin_MultipleBatches_ConsumesOldestFirst()
    {
        var db = CreateInMemoryContext();
        var service = CreateService(db);
        var oldest = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
        var (user, _) = await SeedUserAsync(
            db,
            batches:
            [
                new()
                {
                    Amount = 2,
                    RemainingAmount = 2,
                    ExpiredAt = DateTimeOffset.UtcNow.AddDays(10),
                    CreatedAt = oldest,
                },
                new()
                {
                    Amount = 5,
                    RemainingAmount = 5,
                    ExpiredAt = DateTimeOffset.UtcNow.AddDays(10),
                    CreatedAt = oldest.AddDays(1),
                },
            ]
        );

        var result = await service.DeductCoinAsync(
            user.Id,
            new DeductCoinRequest(4, OrderType.AITarot)
        );

        var batches = db.Wallets.Include(w => w.WhiteCoinBatches).Single().WhiteCoinBatches;
        batches.Single(b => b.CreatedAt == oldest).RemainingAmount.Should().Be(0);
        batches.Single(b => b.CreatedAt == oldest.AddDays(1)).RemainingAmount.Should().Be(3);

        var order = db.Orders.Include(o => o.OrderDetails).Single();
        order.Amount.Should().Be(4);
        order.OrderDetails.Should().HaveCount(2);
        order.OrderDetails.Should().OnlyContain(d => d.Amount == 2);
        order.OrderDetails.Should().ContainSingle(
            d => d.WhiteCoinBatchId == batches.Single(b => b.CreatedAt == oldest).Id
        );
        order.OrderDetails.Should().ContainSingle(
            d => d.WhiteCoinBatchId == batches.Single(b => b.CreatedAt == oldest.AddDays(1)).Id
        );

        result.WhiteCoin.Should().Be(3);
    }

    /// <summary>
    /// Expired or fully-depleted batches are skipped when paying for a reading.
    /// </summary>
    [Fact]
    public async Task DeductCoin_SkipsExpiredAndDepletedBatches()
    {
        var db = CreateInMemoryContext();
        var service = CreateService(db);
        var (user, _) = await SeedUserAsync(
            db,
            batches:
            [
                new()
                {
                    Amount = 5,
                    RemainingAmount = 5,
                    ExpiredAt = DateTimeOffset.UtcNow.AddDays(-1),
                },
                new()
                {
                    Amount = 4,
                    RemainingAmount = 0,
                    ExpiredAt = DateTimeOffset.UtcNow.AddDays(10),
                },
                new()
                {
                    Amount = 2,
                    RemainingAmount = 2,
                    ExpiredAt = DateTimeOffset.UtcNow.AddDays(10),
                },
            ]
        );

        var result = await service.DeductCoinAsync(
            user.Id,
            new DeductCoinRequest(2, OrderType.AITarot)
        );

        var batches = db.Wallets.Include(w => w.WhiteCoinBatches).Single().WhiteCoinBatches;
        var expired = batches.Single(b => b.ExpiredAt < DateTimeOffset.UtcNow);
        expired.RemainingAmount.Should().Be(5);
        var active = batches.Single(b => b.Amount == 2 && b.ExpiredAt >= DateTimeOffset.UtcNow);
        active.RemainingAmount.Should().Be(0);

        db.Orders.Single().Amount.Should().Be(2);
        result.WhiteCoin.Should().Be(0);
    }

    /// <summary>
    /// When the active white coin balance is lower than the requested amount, a
    /// BadRequestException with the insufficientCoins code is thrown and nothing changes.
    /// </summary>
    [Fact]
    public async Task DeductCoin_Insufficient_ThrowsBadRequestAndNoChanges()
    {
        var db = CreateInMemoryContext();
        var service = CreateService(db);
        var (user, _) = await SeedUserAsync(
            db,
            batches:
            [
                new()
                {
                    Amount = 1,
                    RemainingAmount = 1,
                    ExpiredAt = DateTimeOffset.UtcNow.AddDays(10),
                },
            ]
        );

        var act = async () =>
            await service.DeductCoinAsync(
                user.Id,
                new DeductCoinRequest(5, OrderType.AITarot)
            );

        await act.Should()
            .ThrowAsync<BadRequestException>()
            .Where(e => e.ErrorCode == WalletErrorCode.InsufficientCoins);
        db.Wallets.Include(w => w.WhiteCoinBatches)
            .Single()
            .WhiteCoinBatches.Single()
            .RemainingAmount
            .Should()
            .Be(1);
        db.Orders.Should().BeEmpty();
    }

    /// <summary>
    /// A deduction for a user without a wallet throws NotFoundException with the
    /// walletNotFound code and records no order.
    /// </summary>
    [Fact]
    public async Task DeductCoin_WalletNotFound_ThrowsNotFound()
    {
        var db = CreateInMemoryContext();
        var service = CreateService(db);

        var act = async () =>
            await service.DeductCoinAsync(
                Guid.NewGuid(),
                new DeductCoinRequest(1, OrderType.AITarot)
            );

        await act.Should()
            .ThrowAsync<NotFoundException>()
            .Where(e => e.ErrorCode == WalletErrorCode.WalletNotFound);
        db.Orders.Should().BeEmpty();
    }

    /// <summary>
    /// A non-positive amount or an invalid order type is rejected as a BadRequestException
    /// before any persistence.
    /// </summary>
    [Fact]
    public async Task DeductCoin_InvalidRequest_ThrowsBadRequest()
    {
        var db = CreateInMemoryContext();
        var service = CreateService(db);
        var (user, _) = await SeedUserAsync(
            db,
            batches:
            [
                new()
                {
                    Amount = 5,
                    RemainingAmount = 5,
                    ExpiredAt = DateTimeOffset.UtcNow.AddDays(10),
                },
            ]
        );

        var requests = new[]
        {
            new DeductCoinRequest(0, OrderType.AITarot),
            new DeductCoinRequest(-1, OrderType.AITarot),
            new DeductCoinRequest(2, (OrderType)99),
        };

        foreach (var request in requests)
        {
            var act = async () => await service.DeductCoinAsync(user.Id, request);

            await act.Should()
                .ThrowAsync<BadRequestException>()
                .Where(e => e.ErrorCode == WalletErrorCode.InvalidAmount);
        }

        db.Orders.Should().BeEmpty();
        db.Wallets.Include(w => w.WhiteCoinBatches)
            .Single()
            .WhiteCoinBatches.Single()
            .RemainingAmount
            .Should()
            .Be(5);
    }

    #endregion
}