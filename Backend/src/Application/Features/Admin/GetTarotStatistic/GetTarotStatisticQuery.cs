using MediatR;

namespace MyTarotReader.Application.Features.Admin;

public record GetTarotStatisticQuery() : IRequest<GetTarotStatisticResult>;

/// <summary>
/// Aggregated AI tarot reading count for one QuestionType + CardCount group.
/// </summary>
public record GetTarotStatisticAiTarotItem(string QuestionType, int CardCount, int Count);

public record GetTarotStatisticResult(
    int TotalGuestTarotReadings,
    int TotalRegisteredTarotReadings,
    int TotalAiTarotReadings,
    List<GetTarotStatisticAiTarotItem> AiTarotBreakdown
);
