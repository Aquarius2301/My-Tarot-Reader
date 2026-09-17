using Microsoft.EntityFrameworkCore;
using MyTarotReader.Application.Contracts.Persistence;
using MyTarotReader.Domain.Entities;
using MyTarotReader.Infrastructure.Persistence.Configurations;

namespace MyTarotReader.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options)
    : DbContext(options),
        IAppDbContext
{
    public DbSet<AIChatMessage> AIChatMessages { get; set; } = null!;
    public DbSet<AIChatTarotReading> AIChatTarotReadings { get; set; } = null!;
    public DbSet<AITarotReading> AITarotReadings { get; set; } = null!;
    public DbSet<Order> Orders { get; set; } = null!;
    public DbSet<OrderDetail> OrderDetails { get; set; } = null!;
    public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;
    public DbSet<TarotReading> TarotReadings { get; set; } = null!;
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Wallet> Wallets { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new AIChatMessageConfiguration());
        modelBuilder.ApplyConfiguration(new AIChatTarotReadingConfiguration());
        modelBuilder.ApplyConfiguration(new AITarotReadingConfiguration());
        modelBuilder.ApplyConfiguration(new OrderConfiguration());
        modelBuilder.ApplyConfiguration(new OrderDetailConfiguration());
        modelBuilder.ApplyConfiguration(new RefreshTokenConfiguration());
        modelBuilder.ApplyConfiguration(new TarotReadingConfiguration());
        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new WalletConfiguration());
        modelBuilder.ApplyConfiguration(new WhiteCoinBatchConfiguration());
    }
}
