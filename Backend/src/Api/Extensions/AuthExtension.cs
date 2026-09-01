using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MyTarotReader.Api.Helpers;
using MyTarotReader.Application.Settings;

namespace MyTarotReader.Api.Extensions;

/// <summary>
/// Configures JWT bearer authentication. The access token is delivered to the client in an
/// HttpOnly cookie, so the bearer handler reads it from <c>Request.Cookies["accessToken"]</c>
/// rather than the <c>Authorization</c> header.
/// </summary>
public static class AuthExtension
{
    /// <summary>Registers JWT bearer authentication reading the token from the access-token cookie.</summary>
    /// <param name="services">The service collection to extend.</param>
    /// <param name="configuration">Application configuration (the <c>Jwt</c> section).</param>
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
                            )
                            && !string.IsNullOrWhiteSpace(token)
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
