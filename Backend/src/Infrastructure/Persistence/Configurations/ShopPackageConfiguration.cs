using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyTarotReader.Domain.Entities;

namespace MyTarotReader.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configuration for the ShopPackage entity, including the seeded coin packages.
/// </summary>
public class ShopPackageConfiguration : IEntityTypeConfiguration<ShopPackage>
{
    private static readonly DateTimeOffset SeedCreatedAt = new(
        2026,
        1,
        1,
        0,
        0,
        0,
        TimeSpan.Zero
    );

    public void Configure(EntityTypeBuilder<ShopPackage> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Code).IsRequired().HasMaxLength(50);
        builder.Property(x => x.RedCoins).IsRequired();
        builder.Property(x => x.WhiteCoins).IsRequired();
        builder.Property(x => x.OldPrice).IsRequired();
        builder.Property(x => x.Price).IsRequired();
        builder.Property(x => x.IsEnabled).IsRequired().HasDefaultValue(true);

        builder.HasIndex(x => x.Code).IsUnique();

        builder.ToTable(t =>
        {
            t.HasCheckConstraint(
                name: "CK_ShopPackages_RedCoins_NonNegative",
                sql: "\"RedCoins\" >= 0"
            );
            t.HasCheckConstraint(
                name: "CK_ShopPackages_WhiteCoins_NonNegative",
                sql: "\"WhiteCoins\" >= 0"
            );
            t.HasCheckConstraint(
                name: "CK_ShopPackages_OldPrice_NonNegative",
                sql: "\"OldPrice\" >= 0"
            );
            t.HasCheckConstraint(name: "CK_ShopPackages_Price_Positive", sql: "\"Price\" > 0");
        });

        builder.HasData(
            new ShopPackage
            {
                Id = Guid.Parse("55555555-5555-5555-5555-555555555551"),
                Code = "pack-10",
                RedCoins = 10,
                WhiteCoins = 0,
                OldPrice = 10000,
                Price = 10000,
                IsEnabled = true,
                CreatedAt = SeedCreatedAt,
                DeletedAt = null,
            },
            new ShopPackage
            {
                Id = Guid.Parse("55555555-5555-5555-5555-555555555552"),
                Code = "pack-30",
                RedCoins = 30,
                WhiteCoins = 0,
                OldPrice = 27000,
                Price = 27000,
                IsEnabled = true,
                CreatedAt = SeedCreatedAt,
                DeletedAt = null,
            },
            new ShopPackage
            {
                Id = Guid.Parse("55555555-5555-5555-5555-555555555553"),
                Code = "pack-50",
                RedCoins = 50,
                WhiteCoins = 0,
                OldPrice = 40000,
                Price = 40000,
                IsEnabled = true,
                CreatedAt = SeedCreatedAt,
                DeletedAt = null,
            }
        );

        builder.HasQueryFilter(x => x.DeletedAt == null);
    }
}
