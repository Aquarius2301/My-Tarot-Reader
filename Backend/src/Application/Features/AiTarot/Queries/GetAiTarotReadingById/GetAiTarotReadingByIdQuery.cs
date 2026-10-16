using MediatR;

namespace MyTarotReader.Application.Features.AiTarot;

public record GetAiTarotReadingByIdQuery(Guid UserId, Guid ReadingId)
    : IRequest<GetAiTarotReadingResult>;

public record GetAiTarotReadingResult(
    Guid Id,
    int CardCount,
    string Type,
    string Title,
    string Answer,
    List<AiReadingCard> Cards,
    DateTimeOffset CreatedAt
);
