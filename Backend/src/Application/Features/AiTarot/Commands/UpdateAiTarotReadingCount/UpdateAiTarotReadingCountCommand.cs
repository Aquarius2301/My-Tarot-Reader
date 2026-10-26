using MediatR;

namespace MyTarotReader.Application.Features.AiTarot;

public record UpdateAiTarotReadingCountCommand(
    Guid Id,
    int CardCount,
    int WhiteCoins,
    List<string> PositionsEn,
    List<string> PositionsVi,
    int MinWords,
    int MaxWords,
    bool IsEnabled
) : IRequest<Unit>;
