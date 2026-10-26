using MediatR;

namespace MyTarotReader.Application.Features.AiTarot;

public record DeleteAiTarotReadingTypeCommand(Guid Id) : IRequest<Unit>;
