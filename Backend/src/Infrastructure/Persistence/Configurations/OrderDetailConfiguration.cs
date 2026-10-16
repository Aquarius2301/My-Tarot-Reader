using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyTarotReader.Domain.Entities;

namespace MyTarotReader.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configuration for the OrderDetail entity.
/// </summary>
public class OrderDetailConfiguration : IEntityTypeConfiguration<OrderDetail>
{
    public void Configure(EntityTypeBuilder<OrderDetail> builder)
    {
        builder.Property(x => x.Amount).IsRequired();

        builder
            .HasOne(x => x.Order)
            .WithMany(t => t.OrderDetails)
            .HasForeignKey(x => x.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        // OrderDetail → WhiteCoinBatch uses Restrict to avoid multiple-cascade-pat
        // (Wallet → WhiteCoinBatch also targets WhiteCoinBatch).
        builder
            .HasOne(x => x.WhiteCoinBatch)
            .WithMany()
            .HasForeignKey(x => x.WhiteCoinBatchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasQueryFilter(x => x.DeletedAt == null);
    }
}
