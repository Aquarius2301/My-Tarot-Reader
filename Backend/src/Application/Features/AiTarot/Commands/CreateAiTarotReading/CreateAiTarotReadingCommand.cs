using MediatR;

namespace MyTarotReader.Application.Features.AiTarot;

public record CreateAiTarotReadingCommand(
    int CardCount,
    string Type,
    string Locale,
    List<AiReadingCard> Cards,
    Guid UserId
) : IRequest<CreateAiTarotReadingResult>;

public record CreateAiTarotReadingResult(Guid Id);
