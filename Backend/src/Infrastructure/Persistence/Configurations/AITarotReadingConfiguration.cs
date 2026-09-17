using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyTarotReader.Domain.Entities;

namespace MyTarotReader.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configuration for the AITarotReading entity.
/// </summary>
public class AITarotReadingConfiguration : IEntityTypeConfiguration<AITarotReading>
{
    public void Configure(EntityTypeBuilder<AITarotReading> builder)
    {
        builder.Property(x => x.CardCount).IsRequired().HasConversion<string>().HasMaxLength(10);
        builder.Property(x => x.QuestionType).IsRequired().HasConversion<string>().HasMaxLength(10);
        builder.Property(x => x.Answer).IsRequired();
        builder.Property(x => x.Cards).IsRequired().HasMaxLength(2000);

        builder
            .HasOne(a => a.User)
            .WithMany(u => u.AITarotReadings)
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasQueryFilter(u => u.DeletedAt == null);
    }
}
