using MyTarotReader.Domain.Enums;

namespace MyTarotReader.Application.Contracts.Services;

/// <summary>
/// Request for Google OAuth login
/// </summary>
/// <param name="Credential">The Google OAuth credential.</param>
/// <param name="Locale">The optional locale information.</param>
public record GoogleLoginRequest(string Credential, string? Locale);

/// <summary>
/// Result of Google OAuth login.
/// </summary>
public record GoogleLoginResult(
    string AccessToken,
    string RefreshToken,
    int AccessTokenMinutes,
    int RefreshTokenDays
);

/// <summary>
/// Result of retrieving the current user's information.
/// </summary>
public record GetCurrentUserResult(
    Guid Id,
    string FullName,
    string Email,
    string Picture,
    int WhiteCoin,
    int RedCoin,
    UserRole Role
);

public interface IAuthService
{
    /// <summary>
    /// Validate credential and login or register user with Google OAuth.
    /// </summary>
    /// <param name="request"><see cref="GoogleLoginRequest"/> containing the Google OAuth credential and optional locale.</param>
    /// <param name="deviceFingerprint">The fingerprint of the device being used for the login.</param>
    /// <returns><see cref="GoogleLoginResult"/> containing new tokens and expiration information.</returns>
    /// <remarks>
    /// If a new user is created, the user will receive a welcome mail and initial amount of white coins.
    /// </remarks>
    Task<GoogleLoginResult> GoogleLoginAsync(
        GoogleLoginRequest request,
        string deviceFingerprint,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Refreshes the access token using the provided refresh token.
    /// </summary>
    /// <param name="refreshToken">The refresh token to validate and rotate.</param>
    /// <param name="deviceFingerprint">The fingerprint of the device being used for the login.</param>
    /// <returns><see cref="GoogleLoginResult"/> containing new tokens and expiration information.</returns>
    Task<GoogleLoginResult> RefreshAsync(
        string refreshToken,
        string deviceFingerprint,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Logs out the user by invalidating the provided refresh token.
    /// </summary>
    /// <param name="refreshToken">The refresh token to invalidate.</param>
    Task LogoutAsync(string refreshToken, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the current user's information based on the provided user ID.
    /// </summary>
    /// <param name="userId">The unique identifier of the authenticated user.</param>
    /// <returns><see cref="GetCurrentUserResult"/> containing the user's information.</returns>
    Task<GetCurrentUserResult> GetCurrentUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    );
}
