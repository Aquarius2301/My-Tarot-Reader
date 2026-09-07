using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyTarotReader.Domain.Entities;

namespace MyTarotReader.Infrastructure.Persistence.Configurations;

/// <summary>Entity configuration for <see cref="ChatMessage"/>.</summary>
public class ChatMessageConfiguration : IEntityTypeConfiguration<ChatMessage>
{
    public void Configure(EntityTypeBuilder<ChatMessage> builder)
    {
        builder.Property(x => x.Role).IsRequired().HasConversion<string>().HasMaxLength(10);
        builder.Property(x => x.Text).IsRequired();

        builder
            .HasOne(m => m.ChatHistory)
            .WithMany(h => h.Messages)
            .HasForeignKey(m => m.ChatId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(m => m.ChatId);

        builder.HasQueryFilter(u => u.DeletedAt == null);
    }
}
