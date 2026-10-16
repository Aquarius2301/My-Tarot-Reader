using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyTarotReader.Domain.Entities;

namespace MyTarotReader.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configuration for the AIChatTarotReading entity.
/// </summary>
public class AIChatTarotReadingConfiguration : IEntityTypeConfiguration<AIChatTarotReading>
{
    public void Configure(EntityTypeBuilder<AIChatTarotReading> builder)
    {
        builder.Property(x => x.Status).IsRequired().HasConversion<string>().HasMaxLength(20);

        builder
            .HasOne(a => a.User)
            .WithMany(u => u.AIChatTarotReadings)
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasMany(a => a.AIChatMessages)
            .WithOne(m => m.AIChatTarotReading)
            .HasForeignKey(m => m.ChatId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasQueryFilter(u => u.DeletedAt == null);
    }
}
