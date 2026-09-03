using MyTarotReader.Application.Contracts.Services;
using MyTarotReader.Infrastructure.Services;

namespace MyTarotReader.Api.Extensions;

/// <summary>
/// Extension methods for registering outbound HTTP clients.
/// </summary>
public static class HttpExtension
{
    /// <summary>
    /// Registers the typed HttpClient used by the AI tarot service and wires up
    /// <see cref="IAiTarotService"/> to its <see cref="AiTarotService"/> implementation.
    /// </summary>
    /// <param name="services">The service collection to extend.</param>
    public static void AddHttpClients(this IServiceCollection services)
    {
        services.AddHttpClient<IAiTarotService, AiTarotService>();
    }
}
