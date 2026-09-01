using Microsoft.Extensions.Configuration;
using MyTarotReader.Application.Contracts.Persistence;
using MyTarotReader.Application.Contracts.Services;
using MyTarotReader.Infrastructure.Persistence;
using MyTarotReader.Infrastructure.Services;
using StackExchange.Redis;

namespace MyTarotReader.Api.Extensions;

/// <summary>
/// Central registration of application services into the dependency-injection container.
/// </summary>
public static class DIExtension
{
    /// <summary>
    /// Registers the application's services (DbContext contract, tarot service) with the container.
    /// </summary>
    /// <param name="services">The service collection to extend.</param>
    public static IServiceCollection AddDIExtensions(this IServiceCollection services)
    {
        services.AddScoped<IAppDbContext, AppDbContext>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ITarotService, TarotService>();
        services.AddScoped<IHistoryService, HistoryService>();

        return services;
    }
}
