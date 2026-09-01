using Microsoft.EntityFrameworkCore;
using MyTarotReader.Application.Contracts.Persistence;
using MyTarotReader.Domain.Entities;
using MyTarotReader.Infrastructure.Persistence.Configurations;

namespace MyTarotReader.Infrastructure.Persistence;

public class AppDbContext : DbContext, IAppDbContext
{
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;
    public DbSet<Wallet> Wallets { get; set; } = null!;
    public DbSet<ReadHistory> ReadHistories { get; set; } = null!;

    // Generic Set implementation required by IAppDbContext
    public new DbSet<T> Set<T>() where T : class => base.Set<T>();

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new UserConfiguration());
        modelBuilder.ApplyConfiguration(new WalletConfiguration());
        modelBuilder.ApplyConfiguration(new RefreshTokenConfiguration());
        modelBuilder.ApplyConfiguration(new ReadHistoryConfiguration());
    }
}
