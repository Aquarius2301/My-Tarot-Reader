using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using MyTarotReader.Application.Common.Exceptions;

namespace MyTarotReader.Api.Helpers;

/// <summary>
/// Extracts identity information from the current authenticated user.
/// </summary>
public class JwtHelper
{
    /// <summary>
    /// Gets the authenticated user's identifier from the JWT claims.
    /// </summary>
    /// <param name="httpContext">The current HTTP context.</param>
    /// <returns>The authenticated user's <see cref="Guid"/> identifier.</returns>
    /// <exception cref="UnauthorizedException">
    /// Thrown when the user is not authenticated or the identifier claim is missing or invalid.
    /// </exception>
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
