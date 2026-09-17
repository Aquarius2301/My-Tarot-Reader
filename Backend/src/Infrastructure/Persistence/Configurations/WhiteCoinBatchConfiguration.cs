using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyTarotReader.Domain.Entities;

namespace MyTarotReader.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configuration for the WhiteCoinBatch entity.
/// </summary>
public class WhiteCoinBatchConfiguration : IEntityTypeConfiguration<WhiteCoinBatch>
{
    public void Configure(EntityTypeBuilder<WhiteCoinBatch> builder)
    {
        builder.Property(x => x.Amount).IsRequired();
        builder.Property(x => x.RemainingAmount).IsRequired().HasDefaultValue(0);
        builder.Property(x => x.ExpiredAt).IsRequired();

        builder.ToTable(t =>
        {
            t.HasCheckConstraint(
                name: "CK_WhiteCoinBatches_Amount_NonNegative",
                sql: "\"Amount\" >= 0"
            );
            t.HasCheckConstraint(
                name: "CK_WhiteCoinBatches_RemainingAmount_NonNegative",
                sql: "\"RemainingAmount\" >= 0"
            );
        });

        builder
            .HasOne(x => x.Wallet)
            .WithMany(w => w.WhiteCoinBatches)
            .HasForeignKey(x => x.WalletId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasQueryFilter(x => x.DeletedAt == null);
    }
}
