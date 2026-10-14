using MediatR;

namespace MyTarotReader.Application.Features.Tarot;

public record RemoveDrawnForAuthCommand(Guid UserId, Guid ReadingId) : IRequest;
