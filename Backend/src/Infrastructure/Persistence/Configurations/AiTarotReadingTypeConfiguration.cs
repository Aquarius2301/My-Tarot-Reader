using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyTarotReader.Domain.Entities;

namespace MyTarotReader.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configuration for the AiTarotReadingType entity, including the seeded question types.
/// </summary>
public class AiTarotReadingTypeConfiguration : IEntityTypeConfiguration<AiTarotReadingType>
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

    public void Configure(EntityTypeBuilder<AiTarotReadingType> builder)
    {
        builder.Property(x => x.Code).IsRequired().HasMaxLength(20);
        builder.Property(x => x.NameEn).IsRequired().HasMaxLength(100);
        builder.Property(x => x.NameVi).IsRequired().HasMaxLength(100);
        builder.Property(x => x.IsEnabled).IsRequired().HasDefaultValue(true);

        builder.HasIndex(x => x.Code).IsUnique();

        builder.HasData(
            new AiTarotReadingType
            {
                Id = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000001"),
                Code = "energy",
                NameEn = "Energy",
                NameVi = "Năng lượng",
                IsEnabled = true,
                CreatedAt = SeedCreatedAt,
                DeletedAt = null,
            },
            new AiTarotReadingType
            {
                Id = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000002"),
                Code = "love",
                NameEn = "Love",
                NameVi = "Tình yêu",
                IsEnabled = true,
                CreatedAt = SeedCreatedAt,
                DeletedAt = null,
            },
            new AiTarotReadingType
            {
                Id = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000003"),
                Code = "career",
                NameEn = "Career",
                NameVi = "Sự nghiệp",
                IsEnabled = true,
                CreatedAt = SeedCreatedAt,
                DeletedAt = null,
            },
            new AiTarotReadingType
            {
                Id = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000004"),
                Code = "money",
                NameEn = "Money",
                NameVi = "Tài chính",
                IsEnabled = true,
                CreatedAt = SeedCreatedAt,
                DeletedAt = null,
            }
        );

        builder.HasQueryFilter(x => x.DeletedAt == null);
    }
}
