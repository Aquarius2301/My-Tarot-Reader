using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyTarotReader.Domain.Entities;

namespace MyTarotReader.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configuration for the AITarotReadingCount entity, including the seeded spread settings.
/// </summary>
public class AITarotReadingCountConfiguration : IEntityTypeConfiguration<AITarotReadingCount>
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

    public void Configure(EntityTypeBuilder<AITarotReadingCount> builder)
    {
        builder.Property(x => x.CardCount).IsRequired();
        builder.Property(x => x.WhiteCoins).IsRequired();
        builder.Property(x => x.PositionsEn).IsRequired();
        builder.Property(x => x.PositionsVi).IsRequired();
        builder.Property(x => x.MinWords).IsRequired();
        builder.Property(x => x.MaxWords).IsRequired();
        builder.Property(x => x.IsEnabled).IsRequired().HasDefaultValue(true);

        builder.HasIndex(x => x.CardCount).IsUnique();

        builder.ToTable(t =>
        {
            t.HasCheckConstraint(
                name: "CK_AITarotReadingCounts_WhiteCoins_NonNegative",
                sql: "\"WhiteCoins\" >= 0"
            );
            t.HasCheckConstraint(
                name: "CK_AITarotReadingCounts_MinWords_Positive",
                sql: "\"MinWords\" > 0"
            );
        });

        builder.HasData(
            new AITarotReadingCount
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                CardCount = 3,
                WhiteCoins = 2,
                PositionsEn = ["Core energy", "Challenges/obstacles", "Outcome/advice"],
                PositionsVi = ["Năng lượng cốt lõi", "Thử thách/trở ngại", "Kết quả/lời khuyên"],
                MinWords = 300,
                MaxWords = 400,
                IsEnabled = true,
                CreatedAt = SeedCreatedAt,
                DeletedAt = null,
            },
            new AITarotReadingCount
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                CardCount = 5,
                WhiteCoins = 3,
                PositionsEn =
                [
                    "Core energy",
                    "Challenges/obstacles",
                    "Your strength",
                    "Future",
                    "Outcome/advice",
                ],
                PositionsVi =
                [
                    "Năng lượng cốt lõi",
                    "Thử thách/trở ngại",
                    "Điểm mạnh của bạn",
                    "Tương lai",
                    "Kết quả/lời khuyên",
                ],
                MinWords = 450,
                MaxWords = 600,
                IsEnabled = true,
                CreatedAt = SeedCreatedAt,
                DeletedAt = null,
            },
            new AITarotReadingCount
            {
                Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                CardCount = 7,
                WhiteCoins = 4,
                PositionsEn =
                [
                    "Core energy",
                    "Challenges/obstacles",
                    "Your strength",
                    "Hidden influences",
                    "The way to face it",
                    "Future",
                    "Outcome/advice",
                ],
                PositionsVi =
                [
                    "Năng lượng cốt lõi",
                    "Thử thách/trở ngại",
                    "Điểm mạnh của bạn",
                    "Ảnh hưởng ẩn",
                    "Cách đối mặt",
                    "Tương lai",
                    "Kết quả/lời khuyên",
                ],
                MinWords = 650,
                MaxWords = 800,
                IsEnabled = true,
                CreatedAt = SeedCreatedAt,
                DeletedAt = null,
            },
            new AITarotReadingCount
            {
                Id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                CardCount = 10,
                WhiteCoins = 5,
                PositionsEn =
                [
                    "Core energy",
                    "Challenges/obstacles",
                    "What to focus on",
                    "Past",
                    "Your strength",
                    "Near future",
                    "Suggested approach",
                    "What you need to know",
                    "Hopes/fears",
                    "Outcome/advice",
                ],
                PositionsVi =
                [
                    "Năng lượng cốt lõi",
                    "Thử thách/trở ngại",
                    "Điều cần tập trung",
                    "Quá khứ",
                    "Điểm mạnh của bạn",
                    "Tương lai gần",
                    "Hướng tiếp cận",
                    "Điều bạn cần biết",
                    "Hy vọng/nỗi sợ",
                    "Kết quả/lời khuyên",
                ],
                MinWords = 800,
                MaxWords = 1000,
                IsEnabled = true,
                CreatedAt = SeedCreatedAt,
                DeletedAt = null,
            }
        );

        builder.HasQueryFilter(x => x.DeletedAt == null);
    }
}
