using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyTarotReader.Domain.Entities;

namespace MyTarotReader.Infrastructure.Persistence.Configurations;

/// <summary>
/// Configuration for the AIDeepTarotReading entity.
/// </summary>
public class AIDeepTarotReadingConfiguration : IEntityTypeConfiguration<AIDeepTarotReading>
{
    public void Configure(EntityTypeBuilder<AIDeepTarotReading> builder)
    {
        builder.Property(x => x.Topic).IsRequired().HasConversion<string>().HasMaxLength(30);
        builder.Property(x => x.Answer).IsRequired();
        builder.Property(x => x.AnswerSummary).IsRequired().HasMaxLength(500);
        builder.Property(x => x.Cards).IsRequired().HasMaxLength(2000);

        builder
            .HasOne(a => a.User)
            .WithMany(u => u.AIDeepTarotReadings)
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasQueryFilter(u => u.DeletedAt == null);
    }
}
