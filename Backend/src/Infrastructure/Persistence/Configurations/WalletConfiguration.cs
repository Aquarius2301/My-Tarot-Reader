using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyTarotReader.Domain.Entities;

namespace MyTarotReader.Infrastructure.Persistence.Configurations;

/// <summary>Entity configuration for <see cref="Wallet"/>.</summary>
public class WalletConfiguration : IEntityTypeConfiguration<Wallet>
{
    public void Configure(EntityTypeBuilder<Wallet> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.RedCoin).IsRequired().HasDefaultValue(0);
        builder.Property(x => x.UpdatedAt).IsRequired();

        builder.HasIndex(x => x.UserId).IsUnique();

        // One-to-one with User (User is the dependent, keyed by UserId).
        builder
            .HasOne(x => x.User)
            .WithOne(u => u.Wallet)
            .HasForeignKey<Wallet>(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Wallet → WhiteCoinBatch uses Restrict to avoid multiple-cascade-path
        // (TransactionDetail → WhiteCoinBatch also targets WhiteCoinBatch).
        builder
            .HasMany(x => x.WhiteCoinBatches)
            .WithOne()
            .HasForeignKey(x => x.WalletId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasQueryFilter(x => x.DeletedAt == null);
    }
}