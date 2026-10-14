using MediatR;
using MyTarotReader.Domain.Enums;

namespace MyTarotReader.Application.Features.AiTarot;

public record CreateAiTarotReadingCommand(
    CardCount CardCount,
    QuestionType Type,
    string Locale,
    List<AiReadingCard> Cards,
    Guid UserId
) : IRequest<Guid>;
