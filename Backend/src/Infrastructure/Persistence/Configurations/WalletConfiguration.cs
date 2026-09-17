using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyTarotReader.Domain.Entities;

namespace MyTarotReader.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configuration for the Wallet entity.
/// </summary>
public class WalletConfiguration : IEntityTypeConfiguration<Wallet>
{
    public void Configure(EntityTypeBuilder<Wallet> builder)
    {
        builder.Property(x => x.RedCoin).IsRequired().HasDefaultValue(0);
        builder.Property(x => x.UpdatedAt).IsRequired();

        builder.ToTable(t =>
            t.HasCheckConstraint(name: "CK_Wallets_RedCoin_NonNegative", sql: "\"RedCoin\" >= 0")
        );

        builder
            .HasOne(x => x.User)
            .WithOne(u => u.Wallet)
            .HasForeignKey<Wallet>(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasQueryFilter(x => x.DeletedAt == null);
    }
}
