using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MyTarotReader.Infrastructure.Persistence;

namespace MyTarotReader.Api.Extensions;

/// <summary>
/// Registers the application's EF Core DbContext with SQL Server.
/// </summary>
public static class DatabaseExtension
{
    /// <summary>
    /// Adds the <see cref="AppDbContext"/> to the service collection using the
    /// <c>DefaultConnection</c> connection string. Pooled registration reuses
    /// context instances to reduce per-request allocation once SQL reads exist.
    /// </summary>
    /// <param name="services">The service collection to extend.</param>
    /// <param name="configuration">Configuration providing the DefaultConnection string.</param>
    public static void AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString =
            configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("DefaultConnection is not configured.");

        services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));
    }
}
