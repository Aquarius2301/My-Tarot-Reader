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
    /// <see cref="IAiTarotService"/> to its <see cref="AiTarotService"/> implementation,
    /// and <see cref="IAiChatService"/> to its <see cref="AiChatService"/> implementation.
    /// </summary>
    /// <param name="services">The service collection to extend.</param>
    public static void AddHttpClients(this IServiceCollection services)
    {
        services.AddHttpClient<IAiTarotService, AiTarotService>();
        services.AddHttpClient<IAiChatService, AiChatService>();
    }
}
