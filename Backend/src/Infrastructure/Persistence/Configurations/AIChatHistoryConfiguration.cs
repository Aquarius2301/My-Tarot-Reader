using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyTarotReader.Domain.Entities;

namespace MyTarotReader.Infrastructure.Persistence.Configurations;

public class AIChatHistoryConfiguration : IEntityTypeConfiguration<AIChatHistory>
{
    public void Configure(EntityTypeBuilder<AIChatHistory> builder)
    {
        builder.Property(x => x.Status).IsRequired().HasConversion<string>().HasMaxLength(20);

        builder
            .HasOne(a => a.User)
            .WithMany(u => u.AIChatHistories)
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasMany(a => a.Messages)
            .WithOne(m => m.ChatHistory)
            .HasForeignKey(m => m.ChatId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(a => a.UserId);

        builder.HasQueryFilter(u => u.DeletedAt == null);
    }
}
