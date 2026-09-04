using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyTarotReader.Domain.Entities;

namespace MyTarotReader.Infrastructure.Persistence.Configurations;

/// <summary>Entity configuration for <see cref="ChatMessage"/>.</summary>
public class ChatMessageConfiguration : IEntityTypeConfiguration<ChatMessage>
{
    public void Configure(EntityTypeBuilder<ChatMessage> builder)
    {
        builder.Property(x => x.Role).IsRequired().HasMaxLength(10);
        builder.Property(x => x.Text).IsRequired();
        builder.Property(x => x.Sequence).IsRequired();

        builder
            .HasOne(m => m.History)
            .WithMany(h => h.Messages)
            .HasForeignKey(m => m.HistoryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(m => new { m.HistoryId, m.Sequence });
    }
}
