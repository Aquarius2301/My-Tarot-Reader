using MyTarotReader.Domain.Enums;

namespace MyTarotReader.Application.Contracts.Services;

/// <summary>
/// Request to mint a development access token for a JWT-protected endpoint.
/// </summary>
/// <param name="Role">
/// Optional role to assign to the development user. When null the user keeps its
/// current role, or <see cref="UserRole.Registered"/> when first seeded.
/// </param>
public record CreateDevTokenRequest(UserRole? Role);

/// <summary>
/// Result of minting a development access token.
/// </summary>
/// <param name="AccessToken">JWT to paste into Swagger UI's "Authorize" dialog.</param>
/// <param name="RefreshToken">Refresh token, also set as an HttpOnly cookie.</param>
/// <param name="AccessTokenMinutes">Access token lifetime in minutes.</param>
/// <param name="RefreshTokenDays">Refresh token lifetime in days.</param>
/// <param name="UserId">Identifier of the development user, handy for path parameters.</param>
/// <param name="Email">Email of the development user.</param>
/// <param name="FullName">Display name of the development user.</param>
/// <param name="WhiteCoin">Current spendable white coin balance.</param>
/// <param name="RedCoin">Current red coin balance.</param>
/// <param name="Role">Role currently assigned to the development user.</param>
public record CreateDevTokenResult(
    string AccessToken,
    string RefreshToken,
    int AccessTokenMinutes,
    int RefreshTokenDays,
    Guid UserId,
    string Email,
    string FullName,
    int WhiteCoin,
    int RedCoin,
    UserRole Role
);

/// <summary>
/// Issues tokens for a seeded development user so authenticated endpoints can be
/// tested without Google OAuth.
/// </summary>
/// <remarks>
/// Only ever exposed through a <c>[DevelopmentOnly]</c> controller, which is removed
/// from the application model outside the Development environment.
/// </remarks>
public interface IDevAuthService
{
    /// <summary>
    /// Finds or seeds the development user and issues an access and refresh token pair for it.
    /// </summary>
    /// <param name="request"><see cref="CreateDevTokenRequest"/> with the optional role override.</param>
    /// <param name="deviceFingerprint">The device the tokens are bound to, from the "X-Device-Id" header.</param>
    /// <returns><see cref="CreateDevTokenResult"/> with the tokens and the seeded user's identity.</returns>
    Task<CreateDevTokenResult> CreateDevTokenAsync(
        CreateDevTokenRequest request,
        string deviceFingerprint,
        CancellationToken cancellationToken = default
    );
}
