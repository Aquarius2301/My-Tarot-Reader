using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using MyTarotReader.Domain.Entities;

namespace MyTarotReader.Application.Contracts.Persistence;

/// <summary>
/// Defines the application's database context contract for persistence operations.
/// </summary>
public interface IAppDbContext
{
    DbSet<AIChatMessage> AIChatMessages { get; set; }
    DbSet<AIChatTarotReading> AIChatTarotReadings { get; set; }
    DbSet<AITarotReading> AITarotReadings { get; set; }
    DbSet<Order> Orders { get; set; }
    DbSet<OrderDetail> OrderDetails { get; set; }
    DbSet<RefreshToken> RefreshTokens { get; set; }
    DbSet<Streak> Streaks { get; set; }
    DbSet<TarotReading> TarotReadings { get; set; }
    DbSet<User> Users { get; set; }
    DbSet<Wallet> Wallets { get; set; }
    DbSet<WhiteCoinBatch> WhiteCoinBatches { get; set; }

    DatabaseFacade Database { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
