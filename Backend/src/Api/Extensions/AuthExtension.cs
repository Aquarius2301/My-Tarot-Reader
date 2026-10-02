using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using MyTarotReader.Api.Helpers;
using MyTarotReader.Application.Settings;

namespace MyTarotReader.Api.Extensions;

/// <summary>
/// Configures JWT bearer authentication.
/// </summary>
public static class AuthExtension
{
    /// <summary>
    /// Registers JWT bearer authentication configured to read the access token from the
    /// <see cref="CookieHelper.AccessTokenCookieName"/> cookie when the request carries
    /// no <c>Authorization</c> header.
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

                // Fall back to the HttpOnly cookie when the request carries no Authorization
                // header, since the JWT bearer handler has already parsed the header into
                // context.Token by the time this runs. An explicit header wins so that a
                // stale accessToken cookie cannot silently authenticate as another user.
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        if (string.IsNullOrWhiteSpace(context.Token))
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
                        }

                        return Task.CompletedTask;
                    },
                };
            });

        return services;
    }
}
