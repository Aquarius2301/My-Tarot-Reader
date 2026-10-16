using FluentAssertions;
using MyTarotReader.Application.Features.Shop;
using MyTarotReader.Domain.Entities;
using MyTarotReader.Infrastructure.Persistence;
using MyTarotReader.UnitTest.Common;
using Xunit;

namespace MyTarotReader.UnitTest.Features.Shop;

public class GetPackagesQueryHandlerTests
{
    private static GetPackagesQueryHandler CreateSut(AppDbContext db) => new(db);

    private static void SeedPackage(
        AppDbContext db,
        string code,
        int redCoins,
        int whiteCoins,
        int oldPrice,
        int price,
        bool isEnabled = true,
        bool deleted = false
    )
    {
        db.ShopPackages.Add(
            new ShopPackage
            {
                Code = code,
                RedCoins = redCoins,
                WhiteCoins = whiteCoins,
                OldPrice = oldPrice,
                Price = price,
                IsEnabled = isEnabled,
                CreatedAt = DateTimeOffset.UtcNow,
                DeletedAt = deleted ? DateTimeOffset.UtcNow : null,
            }
        );
        db.SaveChanges();
    }

    [Fact]
    public async Task GetPackages_EnabledPackage_ReturnsMappedItem()
    {
        // Arrange
        var db = TestDbContextFactory.Create();
        SeedPackage(db, "pack-10", 10, 2, 10000, 9000);
        var sut = CreateSut(db);

        // Act
        var result = await sut.Handle(new GetPackagesQuery(), CancellationToken.None);

        // Assert
        result.Packages.Should().ContainSingle().Which.Should().Be(
            new GetPackagesItem("pack-10", 10, 2, 10000, 9000)
        );
    }

    [Fact]
    public async Task GetPackages_MultiplePackages_ReturnsOrderedByPrice()
    {
        // Arrange
        var db = TestDbContextFactory.Create();
        SeedPackage(db, "pack-large", 50, 0, 40000, 40000);
        SeedPackage(db, "pack-small", 10, 0, 10000, 9000);
        SeedPackage(db, "pack-medium", 30, 0, 27000, 27000);
        var sut = CreateSut(db);

        // Act
        var result = await sut.Handle(new GetPackagesQuery(), CancellationToken.None);

        // Assert
        result.Packages
            .Select(p => p.Code)
            .Should()
            .Equal("pack-small", "pack-medium", "pack-large");
    }

    [Fact]
    public async Task GetPackages_DisabledPackage_IsExcluded()
    {
        // Arrange
        var db = TestDbContextFactory.Create();
        SeedPackage(db, "pack-active", 10, 0, 10000, 10000);
        SeedPackage(db, "pack-paused", 30, 0, 27000, 27000, isEnabled: false);
        var sut = CreateSut(db);

        // Act
        var result = await sut.Handle(new GetPackagesQuery(), CancellationToken.None);

        // Assert
        result.Packages.Should().ContainSingle().Which.Code.Should().Be("pack-active");
    }

    [Fact]
    public async Task GetPackages_SoftDeletedPackage_IsExcluded()
    {
        // Arrange
        var db = TestDbContextFactory.Create();
        SeedPackage(db, "pack-kept", 10, 0, 10000, 10000);
        SeedPackage(db, "pack-removed", 30, 0, 27000, 27000, deleted: true);
        var sut = CreateSut(db);

        // Act
        var result = await sut.Handle(new GetPackagesQuery(), CancellationToken.None);

        // Assert
        result.Packages.Should().ContainSingle().Which.Code.Should().Be("pack-kept");
    }
}
