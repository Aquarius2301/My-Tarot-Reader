using MediatR;

namespace MyTarotReader.Application.Features.Auth;

public record GoogleLoginCommand(string Credential, string? Locale, string DeviceFingerprint)
    : IRequest<GoogleLoginResult>;

public record GoogleLoginResult(
    string AccessToken,
    string RefreshToken,
    int AccessTokenMinutes,
    int RefreshTokenDays
);
