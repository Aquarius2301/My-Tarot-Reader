using MediatR;
using MyTarotReader.Domain.Enums;

namespace MyTarotReader.Application.Features.AiTarot;

public record GetAiTarotReadingByIdQuery(Guid UserId, Guid ReadingId)
    : IRequest<GetAiTarotReadingResult>;

public record GetAiTarotReadingResult(
    Guid Id,
    CardCount CardCount,
    QuestionType Type,
    string Title,
    string Answer,
    List<AiReadingCard> Cards,
    DateTimeOffset CreatedAt
);
