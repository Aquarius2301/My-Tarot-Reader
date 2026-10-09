using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyTarotReader.Domain.Entities;

namespace MyTarotReader.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configuration for the PaymentOrder entity.
/// </summary>
public class PaymentOrderConfiguration : IEntityTypeConfiguration<PaymentOrder>
{
    public void Configure(EntityTypeBuilder<PaymentOrder> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.OrderCode).IsRequired();
        builder.Property(x => x.PackageCode).IsRequired().HasMaxLength(50);
        builder.Property(x => x.AmountVnd).IsRequired();
        builder.Property(x => x.RedCoins).IsRequired();
        builder.Property(x => x.Status).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(x => x.PayOsPaymentLinkId).HasMaxLength(100);
        builder.Property(x => x.PayOsTransactionReference).HasMaxLength(100);

        builder.ToTable(t =>
            t.HasCheckConstraint(
                name: "CK_PaymentOrders_AmountVnd_Positive",
                sql: "\"AmountVnd\" > 0"
            )
        );
        builder.ToTable(t =>
            t.HasCheckConstraint(
                name: "CK_PaymentOrders_RedCoins_Positive",
                sql: "\"RedCoins\" > 0"
            )
        );

        builder.HasIndex(x => x.OrderCode).IsUnique();
        builder.HasIndex(x => x.UserId);

        builder
            .HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasQueryFilter(x => x.DeletedAt == null);
    }
}
