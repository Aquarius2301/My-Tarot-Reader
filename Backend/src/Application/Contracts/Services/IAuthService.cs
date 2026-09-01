using System;
using MyTarotReader.Application.Dtos;
using MyTarotReader.Application.Exceptions;

namespace MyTarotReader.Application.Contracts.Services;

public interface IAuthService
{
    /// <summary>
    /// Validates a Google credential and issues a new access/refresh token pair.
    /// </summary>
    /// <param name="credential">The Google ID token received from the client.</param>
    /// <param name="deviceFingerprint">The stable device fingerprint used for token binding.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The issued token pair.</returns>
    /// <exception cref="BadRequestException">Thrown when the Google credential is invalid.</exception>
    Task<GoogleLoginResult> GoogleLoginAsync(
        string credential,
        string deviceFingerprint,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Rotates the refresh token for the current device and returns a new token pair.
    /// </summary>
    /// <param name="refreshToken">The refresh token presented by the client.</param>
    /// <param name="deviceFingerprint">The stable device fingerprint used for token binding.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The rotated token pair.</returns>
    /// <exception cref="UnauthorizedException">Thrown when the refresh token is invalid or expired.</exception>
    Task<GoogleLoginResult> RefreshAsync(
        string refreshToken,
        string deviceFingerprint,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Revokes the specified refresh token if it is still active.
    /// </summary>
    /// <param name="refreshToken">The refresh token to revoke.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that completes when the token has been revoked.</returns>
    Task LogoutAsync(string refreshToken, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the authenticated user's profile and wallet information.
    /// </summary>
    /// <param name="userId">The authenticated user's identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The current user profile.</returns>
    /// <exception cref="UnauthorizedException">Thrown when the user does not exist or is inactive.</exception>
    Task<GetCurrentUserResponse> GetCurrentUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    );
}
