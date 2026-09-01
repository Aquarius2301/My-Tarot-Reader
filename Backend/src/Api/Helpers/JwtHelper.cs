using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using MyTarotReader.Application.Exceptions;

namespace MyTarotReader.Api.Helpers;

/// <summary>
/// Helper methods for working with JWT tokens.
/// </summary>
public static class JwtHelper
{
    /// <summary>
    /// Extracts the user identifier (sub claim) from the current HttpContext's authenticated user.
    /// Throws an <see cref="UnauthorizedException"/> if the claim cannot be found.
    /// </summary>
    public static Guid GetUserId(HttpContext httpContext)
    {
        if (httpContext?.User?.Identity?.IsAuthenticated != true)
        {
            throw new UnauthorizedException();
        }

        var claim =
            httpContext.User.FindFirst(JwtRegisteredClaimNames.Sub)
            ?? httpContext.User.FindFirst(ClaimTypes.NameIdentifier);
        if (claim is null || !Guid.TryParse(claim.Value, out var userId))
        {
            throw new UnauthorizedException();
        }
        return userId;
    }
}
