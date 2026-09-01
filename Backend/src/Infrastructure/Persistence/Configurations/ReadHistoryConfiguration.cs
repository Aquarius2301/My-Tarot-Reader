using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MyTarotReader.Domain.Entities;

namespace MyTarotReader.Infrastructure.Persistence.Configurations;

public class ReadHistoryConfiguration : IEntityTypeConfiguration<ReadHistory>
{
    public void Configure(EntityTypeBuilder<ReadHistory> builder)
    {
        builder.HasKey(r => r.Id);
        builder.Property(r => r.CardCode).IsRequired().HasMaxLength(12);
        builder
            .HasOne(r => r.User)
            .WithMany()
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
