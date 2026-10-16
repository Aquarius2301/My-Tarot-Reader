using MediatR;

namespace MyTarotReader.Application.Features.AiTarot;

public record DeleteAiTarotReadingCommand(Guid UserId, Guid ReadingId) : IRequest<Unit>;
