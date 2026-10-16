using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyTarotReader.Domain.Entities;

namespace MyTarotReader.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configuration for the AIChatMessage entity.
/// </summary>
public class AIChatMessageConfiguration : IEntityTypeConfiguration<AIChatMessage>
{
    public void Configure(EntityTypeBuilder<AIChatMessage> builder)
    {
        builder.Property(x => x.Role).IsRequired().HasConversion<string>().HasMaxLength(10);
        builder.Property(x => x.Text).IsRequired();

        builder
            .HasOne(m => m.AIChatTarotReading)
            .WithMany(h => h.AIChatMessages)
            .HasForeignKey(m => m.ChatId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasQueryFilter(u => u.DeletedAt == null);
    }
}
