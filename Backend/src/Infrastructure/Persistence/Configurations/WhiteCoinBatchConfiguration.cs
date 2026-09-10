using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyTarotReader.Domain.Entities;

namespace MyTarotReader.Infrastructure.Persistence.Configurations;

/// <summary>Entity configuration for <see cref="WhiteCoinBatch"/>.</summary>
public class WhiteCoinBatchConfiguration : IEntityTypeConfiguration<WhiteCoinBatch>
{
    public void Configure(EntityTypeBuilder<WhiteCoinBatch> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Amount).IsRequired();
        builder.Property(x => x.RemainingAmount).IsRequired().HasDefaultValue(0);
        builder.Property(x => x.ExpiredAt).IsRequired();

        builder.HasIndex(x => x.WalletId);

        builder.ToTable(t =>
            t.HasCheckConstraint(
                name: "CK_WhiteCoinBatches_RemainingAmount_NonNegative",
                sql: "RemainingAmount >= 0"
            )
        );

        // Wallet → WhiteCoinBatch uses Restrict to avoid multiple-cascade-path from Wallet→WhiteCoinBatch
        // and TransactionDetail→WhiteCoinBatch both targeting WhiteCoinBatch.
        builder
            .HasOne(x => x.Wallet)
            .WithMany(w => w.WhiteCoinBatches)
            .HasForeignKey(x => x.WalletId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasQueryFilter(x => x.DeletedAt == null);
    }
}
