using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyTarotReader.Domain.Entities;

namespace MyTarotReader.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configuration for the TarotReading entity.
/// </summary>
public class TarotReadingConfiguration : IEntityTypeConfiguration<TarotReading>
{
    public void Configure(EntityTypeBuilder<TarotReading> builder)
    {
        builder.Property(r => r.CardCode).IsRequired().HasMaxLength(20);

        builder
            .HasOne(r => r.User)
            .WithMany(r => r.TarotReadings)
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasQueryFilter(u => u.DeletedAt == null);
    }
}
