using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyTarotReader.Domain.Entities;

namespace MyTarotReader.Infrastructure.Persistence.Configurations;

/// <summary>Entity configuration for <see cref="TransactionDetail"/>.</summary>
public class TransactionDetailConfiguration : IEntityTypeConfiguration<TransactionDetail>
{
    public void Configure(EntityTypeBuilder<TransactionDetail> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.TransactionId).IsRequired();
        builder.Property(x => x.Amount).IsRequired();

        builder.HasIndex(x => x.TransactionId);
        builder.HasIndex(x => x.WhiteCoinBatchId);

        builder
            .HasOne(x => x.Transaction)
            .WithMany(t => t.TransactionDetails)
            .HasForeignKey(x => x.TransactionId)
            .OnDelete(DeleteBehavior.Cascade);

        // TransactionDetail → WhiteCoinBatch uses Restrict to avoid multiple-cascade-path
        // (Wallet → WhiteCoinBatch also targets WhiteCoinBatch).
        builder
            .HasOne(x => x.WhiteCoinBatch)
            .WithOne()
            .HasForeignKey<TransactionDetail>(x => x.WhiteCoinBatchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasQueryFilter(x => x.DeletedAt == null);
    }
}