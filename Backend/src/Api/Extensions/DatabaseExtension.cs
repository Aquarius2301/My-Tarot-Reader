using Microsoft.EntityFrameworkCore;
using MyTarotReader.Infrastructure.Persistence;

namespace MyTarotReader.Api.Extensions;

/// <summary>
/// Configures the Entity Framework Core database context.
/// </summary>
public static class DatabaseExtension
{
    /// <summary>
    /// Registers <see cref="AppDbContext"/> using the PostgreSQL (Npgsql) connection string
    /// from configuration.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="configuration">The application configuration, requiring "ConnectionStrings:DefaultConnection".</param>
    /// <exception cref="InvalidOperationException">Thrown when "DefaultConnection" is not configured.</exception>
    public static IServiceCollection AddDatabase(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        var connectionString =
            configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("DefaultConnection is not configured.");

        services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));

        return services;
    }
}
