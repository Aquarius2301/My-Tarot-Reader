namespace MyTarotReader.Application.Contracts.Common;

/// <summary>
/// Represents the payload returned by Google OAuth after validating a credential.
/// </summary>
/// <param name="ProviderKey">The provider key.</param>
/// <param name="Email">The user's email.</param>
/// <param name="Name">The user's name.</param>
/// <param name="Picture">The user's profile picture.</param>
public record GooglePayload(string ProviderKey, string Email, string Name, string Picture);

/// <summary>
/// Validates Google OAuth credentials and extracts the authenticated user's profile.
/// </summary>
public interface IGoogleAuthValidator
{
    /// <summary>
    /// Validates the provided Google OAuth credential and returns the corresponding payload if valid.
    /// </summary>
    /// <param name="credential">The Google OAuth credential (ID token) to validate.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The validated <see cref="GooglePayload"/>, or <c>null</c> if the credential is invalid.</returns>
    /// <exception cref="InvalidJwtException">Thrown when the provided credential is an invalid JWT.</exception>
    /// <exception cref="Exception">Thrown when an unexpected error occurs during the validation process.</exception>
    Task<GooglePayload?> ValidateAsync(
        string credential,
        CancellationToken cancellationToken = default
    );
}
