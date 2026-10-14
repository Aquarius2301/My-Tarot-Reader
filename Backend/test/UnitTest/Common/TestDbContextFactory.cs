using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using MyTarotReader.Infrastructure.Persistence;

namespace MyTarotReader.UnitTest.Common;

/// <summary>
/// Creates an isolated in-memory <see cref="AppDbContext"/> for each test, so tests
/// never leak data into one another. Transactions are logged as no-ops by the
/// InMemory provider; the warning is suppressed to keep handler tests clean.
/// </summary>
public static class TestDbContextFactory
{
    public static AppDbContext Create()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .EnableServiceProviderCaching(false)
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        return new AppDbContext(options);
    }
}
