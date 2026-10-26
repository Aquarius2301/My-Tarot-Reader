using FluentAssertions;
using MyTarotReader.Application.Common.Exceptions;
using MyTarotReader.Application.Constants.Errors;
using MyTarotReader.Application.Features.Shop;
using MyTarotReader.Domain.Entities;
using MyTarotReader.Infrastructure.Persistence;
using MyTarotReader.UnitTest.Common;
using Xunit;

namespace MyTarotReader.UnitTest.Features.Shop;

public class UpdateShopPackageCommandHandlerTests
{
    private static UpdateShopPackageCommandHandler CreateSut(AppDbContext db) => new(db);

    private static ShopPackage SeedPackage(
        AppDbContext db,
        string code,
        bool deleted = false,
        int redCoins = 10,
        int whiteCoins = 0,
        int oldPrice = 10000,
        int price = 10000,
        bool isEnabled = true
    )
    {
        var package = new ShopPackage
        {
            Code = code,
            RedCoins = redCoins,
            WhiteCoins = whiteCoins,
            OldPrice = oldPrice,
            Price = price,
            IsEnabled = isEnabled,
            CreatedAt = DateTimeOffset.UtcNow,
            DeletedAt = deleted ? DateTimeOffset.UtcNow : null,
        };
        db.ShopPackages.Add(package);
        db.SaveChanges();
        return package;
    }

    [Fact]
    public async Task Handle_ExistingPackage_UpdatesFields()
    {
        // Arrange
        var db = TestDbContextFactory.Create();
        var package = SeedPackage(db, "pack-10");
        var sut = CreateSut(db);

        // Act
        await sut.Handle(
            new UpdateShopPackageCommand(package.Id, "  PACK-20  ", 20, 4, 22000, 20000, false),
            CancellationToken.None
        );

        // Assert
        var updated = db.ShopPackages.Single();
        updated.Code.Should().Be("pack-20");
        updated.RedCoins.Should().Be(20);
        updated.WhiteCoins.Should().Be(4);
        updated.OldPrice.Should().Be(22000);
        updated.Price.Should().Be(20000);
        updated.IsEnabled.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_PackageNotFound_ThrowsNotFound()
    {
        // Arrange
        var db = TestDbContextFactory.Create();
        var sut = CreateSut(db);

        // Act
        var act = () =>
            sut.Handle(
                new UpdateShopPackageCommand(Guid.NewGuid(), "pack-10", 10, 0, 10000, 10000, true),
                CancellationToken.None
            );

        // Assert
        var ex = await act.Should().ThrowAsync<NotFoundException>();
        ex.Which.ErrorCode.Should().Be(ShopErrorCode.PackageNotFound);
    }

    [Fact]
    public async Task Handle_ActiveCodeConflict_ThrowsConflict()
    {
        // Arrange
        var db = TestDbContextFactory.Create();
        var package = SeedPackage(db, "pack-10");
        SeedPackage(db, "pack-20");
        var sut = CreateSut(db);

        // Act
        var act = () =>
            sut.Handle(
                new UpdateShopPackageCommand(package.Id, "PACK-20", 10, 0, 10000, 10000, true),
                CancellationToken.None
            );

        // Assert
        var ex = await act.Should().ThrowAsync<ConflictException>();
        ex.Which.ErrorCode.Should().Be(ShopErrorCode.PackageCodeExists);
        db.ShopPackages.Single(p => p.Id == package.Id).Code.Should().Be("pack-10");
    }

    [Fact]
    public async Task Handle_DeletedCodeConflict_ThrowsConflict()
    {
        // Arrange
        var db = TestDbContextFactory.Create();
        var package = SeedPackage(db, "pack-10");
        SeedPackage(db, "pack-20", deleted: true);
        var sut = CreateSut(db);

        // Act
        var act = () =>
            sut.Handle(
                new UpdateShopPackageCommand(package.Id, "pack-20", 10, 0, 10000, 10000, true),
                CancellationToken.None
            );

        // Assert
        var ex = await act.Should().ThrowAsync<ConflictException>();
        ex.Which.ErrorCode.Should().Be(ShopErrorCode.PackageCodeExists);
        db.ShopPackages.Single(p => p.Id == package.Id).Code.Should().Be("pack-10");
    }
}
