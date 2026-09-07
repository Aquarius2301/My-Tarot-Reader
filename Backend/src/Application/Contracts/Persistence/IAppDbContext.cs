using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MyTarotReader.Domain.Entities;

namespace MyTarotReader.Application.Contracts.Persistence;

public interface IAppDbContext
{
    DbSet<User> Users { get; set; }
    DbSet<RefreshToken> RefreshTokens { get; set; }
    DbSet<Wallet> Wallets { get; set; }
    DbSet<ReadHistory> ReadHistories { get; set; }
    DbSet<AIReadHistory> AIReadHistories { get; set; }

    DbSet<ChatMessage> ChatMessages { get; set; }
    DbSet<AIChatHistory> AIChatHistories { get; set; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
