using MyTarotReader.Domain.Entities;

namespace MyTarotReader.Application.Contracts.Common;

/// <summary>
/// Generates access and refresh tokens for authenticated users.
/// </summary>
public interface IJwtTokenGenerator
{
    /// <summary>
    /// Generates an access token for the specified user.
    /// </summary>
    /// <param name="user">The user to generate the access token for.</param>
    /// <returns>The generated access token.</returns>
    string GenerateAccessToken(User user);

    /// <summary>
    /// Generates a refresh token.
    /// </summary>
    /// <returns>The generated refresh token.</returns>
    /// <remarks>
    /// The refresh token is generated as a random 64-byte string, then Base64-encoded
    /// to produce a string representation. This ensures the token is both secure and unique.
    /// </remarks>
    string GenerateRefreshToken();
}
