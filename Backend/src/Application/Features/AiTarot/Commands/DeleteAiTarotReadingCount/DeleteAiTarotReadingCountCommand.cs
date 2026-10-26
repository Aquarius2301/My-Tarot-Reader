using MediatR;

namespace MyTarotReader.Application.Features.AiTarot;

public record DeleteAiTarotReadingCountCommand(Guid Id) : IRequest<Unit>;
