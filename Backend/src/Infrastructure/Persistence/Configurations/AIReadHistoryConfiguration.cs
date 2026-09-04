using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyTarotReader.Domain.Entities;

namespace MyTarotReader.Infrastructure.Persistence.Configurations;

/// <summary>Entity configuration for <see cref="AIReadHistory"/>.</summary>
public class AIReadHistoryConfiguration : IEntityTypeConfiguration<AIReadHistory>
{
    public void Configure(EntityTypeBuilder<AIReadHistory> builder)
    {
        builder.Property(x => x.Status).IsRequired().HasConversion<string>().HasMaxLength(10);
        builder.Property(x => x.CardCount).IsRequired(false).HasConversion<string>().HasMaxLength(10);
        builder.Property(x => x.QuestionType).IsRequired(false).HasConversion<string>().HasMaxLength(10);
        builder.Property(x => x.Question).IsRequired().HasMaxLength(2000);
        builder.Property(x => x.Answer).HasMaxLength(5000);
        builder.Property(x => x.Cards).HasMaxLength(2000);

        builder
            .HasOne(a => a.User)
            .WithMany()
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasMany(a => a.Messages)
            .WithOne(m => m.History)
            .HasForeignKey(m => m.HistoryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(a => a.UserId);
    }
}
