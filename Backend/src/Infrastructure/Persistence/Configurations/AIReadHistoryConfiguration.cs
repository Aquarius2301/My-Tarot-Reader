using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyTarotReader.Domain.Entities;

namespace MyTarotReader.Infrastructure.Persistence.Configurations;

/// <summary>Entity configuration for <see cref="AIReadHistory"/>.</summary>
public class AIReadHistoryConfiguration : IEntityTypeConfiguration<AIReadHistory>
{
    public void Configure(EntityTypeBuilder<AIReadHistory> builder)
    {
        builder.Property(x => x.CardCount).IsRequired().HasConversion<string>().HasMaxLength(10);
        builder.Property(x => x.QuestionType).IsRequired().HasConversion<string>().HasMaxLength(10);
        builder.Property(x => x.Question).HasMaxLength(1000);
        builder.Property(x => x.Answer).IsRequired().HasMaxLength(5000);

        builder
            .HasOne(a => a.User)
            .WithMany()
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(a => a.UserId);
    }
}
