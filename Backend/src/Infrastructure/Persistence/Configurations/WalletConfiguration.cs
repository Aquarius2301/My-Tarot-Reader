using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyTarotReader.Domain.Entities;

namespace MyTarotReader.Infrastructure.Persistence.Configurations;

/// <summary>Entity configuration for <see cref="Wallet"/>.</summary>
public class WalletConfiguration : IEntityTypeConfiguration<Wallet>
{
    public void Configure(EntityTypeBuilder<Wallet> builder)
    {
        builder.Property(w => w.WhiteCoin).HasDefaultValue(0);
        builder.Property(w => w.RedCoin).HasDefaultValue(0);

        // The one-to-one relationship with User is configured on UserConfiguration;
        // the UserId FK must be unique so each user owns exactly one wallet.
        builder.HasIndex(w => w.UserId).IsUnique();
    }
}
