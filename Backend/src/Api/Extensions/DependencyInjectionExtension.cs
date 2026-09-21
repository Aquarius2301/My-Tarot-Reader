using FluentValidation;
using MyTarotReader.Application.Common.Validators;
using MyTarotReader.Application.Contracts.Backgrounds;
using MyTarotReader.Application.Contracts.Common;
using MyTarotReader.Application.Contracts.Persistence;
using MyTarotReader.Application.Contracts.Services;
using MyTarotReader.Infrastructure.Backgrounds;
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
    public static IServiceCollection AddRegister(this IServiceCollection services)
    {
        // Database context
        services.AddScoped<IAppDbContext, AppDbContext>();

        // Common
        services.AddScoped<IGoogleAuthValidator, GoogleAuthValidator>();
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddScoped<IEmailHandler, EmailHandler>();
        services.AddScoped<IEmailSender, EmailSender>();
        services.AddScoped<IEmailTemplateEngine, EmailTemplateEngine>();

        // Background jobs
        services.AddSingleton<IEmailBackgroundQueue, EmailBackgroundQueue>();
        services.AddHostedService<EmailBackgroundWorker>();

        // Services
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IStreakService, StreakService>();
        services.AddScoped<ITarotReadingService, TarotReadingService>();
        services.AddScoped<IWalletService, WalletService>();
        services.AddScoped<IAiTarotReadingService, AiTarotReadingService>();

        // External clients
        services.AddHttpClient<IGeminiClient, GeminiClient>();

        // Validators
        services.AddScoped<
            IValidator<CreateDrawForAuthRequest>,
            CreateDrawForAuthRequestValidator
        >();

        services.AddScoped<
            IValidator<CreateDrawForGuestRequest>,
            CreateDrawForGuestRequestValidator
        >();

        services.AddScoped<IValidator<AddCoinRequest>, AddCoinRequestValidator>();

        services.AddScoped<IValidator<DeductCoinRequest>, DeductCoinRequestValidator>();

        services.AddScoped<
            IValidator<CreateAiTarotReadingRequest>,
            CreateAiTarotReadingRequestValidator
        >();

        return services;
    }
}
