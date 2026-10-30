using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MyTarotReader.Application.Common.Exceptions;
using MyTarotReader.Application.Constants.Errors;
using MyTarotReader.Application.Features.Shop;
using MyTarotReader.Domain.Entities;
using MyTarotReader.Infrastructure.Persistence;
using MyTarotReader.UnitTest.Common;
using Xunit;

namespace MyTarotReader.UnitTest.Features.Shop;

public class CreateShopPackageCommandHandlerTests
{
    private static CreateShopPackageCommandHandler CreateSut(AppDbContext db) => new(db);

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
    public async Task Handle_NewCode_CreatesPackage()
    {
        // Arrange
        var db = TestDbContextFactory.Create();
        var sut = CreateSut(db);

        // Act
        var id = await sut.Handle(
            new CreateShopPackageCommand("  PACK-10  ", 10, 2, 12000, 10000, true),
            CancellationToken.None
        );

        // Assert
        var package = db.ShopPackages.Single();
        package.Id.Should().Be(id);
        package.Code.Should().Be("pack-10");
        package.RedCoins.Should().Be(10);
        package.WhiteCoins.Should().Be(2);
        package.OldPrice.Should().Be(12000);
        package.Price.Should().Be(10000);
        package.IsEnabled.Should().BeTrue();
        package.DeletedAt.Should().BeNull();
    }

    [Fact]
    public async Task Handle_ActiveCode_ThrowsConflict()
    {
        // Arrange
        var db = TestDbContextFactory.Create();
        SeedPackage(db, "pack-10");
        var sut = CreateSut(db);

        // Act
        var act = () =>
            sut.Handle(
                new CreateShopPackageCommand("pack-10", 30, 0, 30000, 30000, true),
                CancellationToken.None
            );

        // Assert
        var ex = await act.Should().ThrowAsync<ConflictException>();
        ex.Which.ErrorCode.Should().Be(ShopErrorCode.PackageCodeExists);
        db.ShopPackages.Should().ContainSingle();
    }

    [Fact]
    public async Task Handle_DeletedCode_RevivesSamePackage()
    {
        // Arrange
        var db = TestDbContextFactory.Create();
        var existing = SeedPackage(db, "pack-10", deleted: true);
        var sut = CreateSut(db);

        // Act
        var id = await sut.Handle(
            new CreateShopPackageCommand("pack-10", 50, 5, 50000, 45000, false),
            CancellationToken.None
        );

        // Assert
        var package = db.ShopPackages.IgnoreQueryFilters().Single();
        id.Should().Be(existing.Id);
        package.Id.Should().Be(existing.Id);
        package.Code.Should().Be("pack-10");
        package.RedCoins.Should().Be(50);
        package.WhiteCoins.Should().Be(5);
        package.OldPrice.Should().Be(50000);
        package.Price.Should().Be(45000);
        package.IsEnabled.Should().BeFalse();
        package.DeletedAt.Should().BeNull();
        package.CreatedAt.Should().Be(existing.CreatedAt);
    }

    [Fact]
    public async Task Handle_DeletedCodeDifferentCaseAndWhitespace_RevivesSamePackage()
    {
        // Arrange
        var db = TestDbContextFactory.Create();
        var existing = SeedPackage(db, "Pack-10", deleted: true);
        var sut = CreateSut(db);

        // Act
        var id = await sut.Handle(
            new CreateShopPackageCommand("  PACK-10  ", 10, 0, 10000, 10000, true),
            CancellationToken.None
        );

        // Assert
        var package = db.ShopPackages.IgnoreQueryFilters().Single();
        id.Should().Be(existing.Id);
        package.Id.Should().Be(existing.Id);
        package.Code.Should().Be("pack-10");
        package.DeletedAt.Should().BeNull();
    }
}
