using MediatR;

namespace MyTarotReader.Application.Features.Tarot;

/// <summary>
/// Request for tarot reading services for authenticated users.
/// </summary>
public record CreateDrawForAuthCommand(string CardCode, bool IsReversed, Guid UserId) : IRequest;
