using FluentAssertions;
using MyTarotReader.Application.Features.Shop;
using MyTarotReader.Domain.Entities;
using MyTarotReader.Infrastructure.Persistence;
using MyTarotReader.UnitTest.Common;
using Xunit;

namespace MyTarotReader.UnitTest.Features.Shop;

public class GetAllShopPackagesQueryHandlerTests
{
    private static GetAllShopPackagesQueryHandler CreateSut(AppDbContext db) => new(db);

    private static void SeedPackage(
        AppDbContext db,
        string code,
        int redCoins,
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
                WhiteCoins = 0,
                OldPrice = price,
                Price = price,
                IsEnabled = isEnabled,
                CreatedAt = DateTimeOffset.UtcNow,
                DeletedAt = deleted ? DateTimeOffset.UtcNow : null,
            }
        );
        db.SaveChanges();
    }

    [Fact]
    public async Task Handle_ActivePackages_ReturnsEnabledAndDisabledOrderedByPrice()
    {
        // Arrange
        var db = TestDbContextFactory.Create();
        SeedPackage(db, "pack-large", 50, 40000);
        SeedPackage(db, "pack-disabled", 20, 20000, isEnabled: false);
        SeedPackage(db, "pack-small", 10, 10000);
        var sut = CreateSut(db);

        // Act
        var result = await sut.Handle(new GetAllShopPackagesQuery(), CancellationToken.None);

        // Assert
        result.Items.Select(i => i.Code).Should().Equal(
            "pack-small",
            "pack-disabled",
            "pack-large"
        );
        result.Items.Single(i => i.Code == "pack-disabled").IsEnabled.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_SoftDeletedPackages_AreExcluded()
    {
        // Arrange
        var db = TestDbContextFactory.Create();
        SeedPackage(db, "pack-kept", 10, 10000);
        SeedPackage(db, "pack-removed", 30, 30000, deleted: true);
        var sut = CreateSut(db);

        // Act
        var result = await sut.Handle(new GetAllShopPackagesQuery(), CancellationToken.None);

        // Assert
        result.Items.Should().ContainSingle().Which.Code.Should().Be("pack-kept");
    }
}
