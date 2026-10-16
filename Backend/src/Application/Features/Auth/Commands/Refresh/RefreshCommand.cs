using MediatR;

namespace MyTarotReader.Application.Features.Auth;

public record RefreshCommand(string RefreshToken, string DeviceFingerprint)
    : IRequest<RefreshResult>;

public record RefreshResult(
    string AccessToken,
    string RefreshToken,
    int AccessTokenMinutes,
    int RefreshTokenDays
);
