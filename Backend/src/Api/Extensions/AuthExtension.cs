using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using MyTarotReader.Api.Helpers;
using MyTarotReader.Application.Settings;

namespace MyTarotReader.Api.Extensions;

/// <summary>
/// Configures JWT bearer authentication.
/// </summary>
public static class AuthenticationExtension
{
    /// <summary>
    /// Registers JWT bearer authentication configured to read the access token
    /// from the <see cref="CookieHelper.AccessTokenCookieName"/> cookie instead of
    /// the Authorization header.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="configuration">The application configuration, requiring a "Jwt" section.</param>
    public static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        var jwt = configuration.GetSection("Jwt").Get<JwtSetting>();

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwt?.Issuer,
                    ValidAudience = jwt?.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwt?.SecretKey ?? string.Empty)
                    ),
                };

                // Pull the token from the HttpOnly cookie instead of the Authorization header.
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        if (
                            context.Request.Cookies.TryGetValue(
                                CookieHelper.AccessTokenCookieName,
                                out var token
                            ) && !string.IsNullOrWhiteSpace(token)
                        )
                        {
                            context.Token = token;
                        }

                        return Task.CompletedTask;
                    },
                };
            });

        return services;
    }
}
