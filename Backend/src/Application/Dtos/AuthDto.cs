using MyTarotReader.Domain.Enums;

namespace MyTarotReader.Application.Dtos;

/// <summary>
/// Request for Google authentication.
/// </summary>
/// <param name="Credential">The Google authentication credential</param>
public record GoogleLoginRequest(string Credential);

/// <summary>
/// Response for successful authentication. Carries both tokens for the controller to write into
/// HttpOnly cookies
/// </summary>
/// <param name="AccessToken">The short-lived JWT access token.</param>
/// <param name="RefreshToken">The long-lived refresh token (stored server-side per device).</param>
public record GoogleLoginResult(string AccessToken, string RefreshToken);

/// <summary>
/// Response for user information including wallet coins.
/// </summary>
/// <param name="Id">The user's ID</param>
/// <param name="Email">The user's email</param>
/// <param name="FullName">The user's full name</param>
/// <param name="Picture">The user's profile picture URL</param>
/// <param name="WhiteCoin">White coin balance</param>
/// <param name="RedCoin">Red coin balance</param>
/// <param name="Role">The user's role</param>
public record GetCurrentUserResponse(
    Guid Id,
    string Email,
    string FullName,
    string Picture,
    int WhiteCoin,
    int RedCoin,
    Role Role
);
