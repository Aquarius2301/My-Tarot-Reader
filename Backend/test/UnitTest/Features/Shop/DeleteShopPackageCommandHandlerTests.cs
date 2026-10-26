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

public class DeleteShopPackageCommandHandlerTests
{
    private static DeleteShopPackageCommandHandler CreateSut(AppDbContext db) => new(db);

    private static ShopPackage SeedPackage(AppDbContext db, bool deleted = false)
    {
        var package = new ShopPackage
        {
            Code = "pack-10",
            RedCoins = 10,
            WhiteCoins = 0,
            OldPrice = 10000,
            Price = 10000,
            IsEnabled = true,
            CreatedAt = DateTimeOffset.UtcNow,
            DeletedAt = deleted ? DateTimeOffset.UtcNow : null,
        };
        db.ShopPackages.Add(package);
        db.SaveChanges();
        return package;
    }

    [Fact]
    public async Task Handle_ExistingPackage_SoftDeletes()
    {
        // Arrange
        var db = TestDbContextFactory.Create();
        var package = SeedPackage(db);
        var sut = CreateSut(db);

        // Act
        await sut.Handle(
            new DeleteShopPackageCommand(package.Id),
            CancellationToken.None
        );

        // Assert
        db.ShopPackages.IgnoreQueryFilters()
            .Single(p => p.Id == package.Id)
            .DeletedAt.Should()
            .NotBeNull();
        db.ShopPackages.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_PackageNotFound_ThrowsNotFound()
    {
        // Arrange
        var db = TestDbContextFactory.Create();
        var sut = CreateSut(db);

        // Act
        var act = () =>
            sut.Handle(new DeleteShopPackageCommand(Guid.NewGuid()), CancellationToken.None);

        // Assert
        var ex = await act.Should().ThrowAsync<NotFoundException>();
        ex.Which.ErrorCode.Should().Be(ShopErrorCode.PackageNotFound);
    }

    [Fact]
    public async Task Handle_AlreadyDeletedPackage_ThrowsNotFound()
    {
        // Arrange
        var db = TestDbContextFactory.Create();
        var package = SeedPackage(db, deleted: true);
        var sut = CreateSut(db);

        // Act
        var act = () =>
            sut.Handle(new DeleteShopPackageCommand(package.Id), CancellationToken.None);

        // Assert
        var ex = await act.Should().ThrowAsync<NotFoundException>();
        ex.Which.ErrorCode.Should().Be(ShopErrorCode.PackageNotFound);
    }
}
