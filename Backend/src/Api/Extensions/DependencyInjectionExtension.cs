using FluentValidation;
using MyTarotReader.Application.Backgrounds;
using MyTarotReader.Application.Common.Behaviors;
using MyTarotReader.Application.Features.Auth;
using MyTarotReader.Application.Persistence;
using MyTarotReader.Application.Services;
using MyTarotReader.Infrastructure.Common;
using MyTarotReader.Infrastructure.Persistence;
using MyTarotReader.Infrastructure.Services;

namespace MyTarotReader.Api.Extensions;

/// <summary>
/// Registers application service and repository implementations for dependency injection.
/// </summary>
public static class DependencyInjectionExtension
{
    /// <summary>
    /// Registers common infrastructure services (DB context, Google auth, JWT, email)
    /// and domain services (auth, tarot reading, history) with scoped lifetime.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <remarks>
    /// <see cref="DevAuthService"/> is registered unconditionally, but its controller
    /// is marked <c>[DevelopmentOnly]</c> so the endpoint only exists in Development.
    /// </remarks>
    public static IServiceCollection AddRegister(this IServiceCollection services)
    {
        // Database context
        services.AddScoped<IAppDbContext, AppDbContext>();

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(GoogleLoginCommand).Assembly);
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        services.AddValidatorsFromAssembly(typeof(GoogleLoginCommandValidator).Assembly);

        services.AddHostedService<EmailBackground>();
        services.AddHostedService<TokenCleanupBackground>();
        services.AddHostedService<PayOsWebhookConfirmBackground>();

        services.AddScoped<IEmailHandlerService, EmailHandlerService>();
        services.AddHttpClient<IEmailSenderService, EmailSenderService>();
        services.AddSingleton<IEmailQueueService, EmailQueueService>();
        services.AddScoped<IEmailTemplateEngineService, EmailTemplateEngineService>();
        services.AddScoped<IGoogleAuthService, GoogleAuthService>();
        services.AddScoped<IJwtTokenGeneratorService, JwtTokenGeneratorService>();
        services.AddScoped<IWalletService, WalletService>();
        services.AddHttpClient<IGeminiClientService, GeminiClientService>();
        services.AddHttpClient<IPayOsClientService, PayOsClientService>();

        return services;
    }
}
