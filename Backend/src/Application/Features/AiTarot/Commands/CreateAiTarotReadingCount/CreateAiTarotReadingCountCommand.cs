using MediatR;

namespace MyTarotReader.Application.Features.AiTarot;

public record CreateAiTarotReadingCountCommand(
    int CardCount,
    int WhiteCoins,
    List<string> PositionsEn,
    List<string> PositionsVi,
    int MinWords,
    int MaxWords,
    bool IsEnabled
) : IRequest<Guid>;
