using MediatR;

namespace MyTarotReader.Application.Features.Auth;

public record LogoutCommand(string RefreshToken) : IRequest<Unit>;
