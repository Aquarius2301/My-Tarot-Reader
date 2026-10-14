using MediatR;
using MyTarotReader.Domain.Enums;

namespace MyTarotReader.Application.Features.Auth;

public record GetCurrentUserQuery(Guid UserId) : IRequest<GetCurrentUserResult>;

public record GetCurrentUserResult(
    Guid Id,
    string FullName,
    string Email,
    string? Picture,
    int WhiteCoin,
    int RedCoin,
    UserRole Role
);
