using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using MyTarotReader.Domain.Entities;

namespace MyTarotReader.Application.Contracts.Persistence;

public interface IAppDbContext
{
    /// <summary>
    /// The database facade, used to manage transactions (e.g. for atomic coin deduction
    /// paired with a later external AI call).
    /// </summary>
    DatabaseFacade Database { get; }

    DbSet<User> Users { get; set; }
    DbSet<RefreshToken> RefreshTokens { get; set; }
    DbSet<Wallet> Wallets { get; set; }
    DbSet<ReadHistory> ReadHistories { get; set; }
    DbSet<AIReadHistory> AIReadHistories { get; set; }
    DbSet<ChatMessage> ChatMessages { get; set; }
    DbSet<AIChatHistory> AIChatHistories { get; set; }
    DbSet<WhiteCoinBatch> WhiteCoinBatches { get; set; }
    DbSet<Transaction> Transactions { get; set; }
    DbSet<TransactionDetail> TransactionDetails { get; set; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
