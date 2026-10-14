using MediatR;

namespace MyTarotReader.Application.Features.AiDeepTarot;

/// <summary>
/// Request to create a 12 months deep tarot reading.
/// </summary>
/// <param name="Locale">The response language of the reading ("en" or "vi").</param>
/// <param name="Cards">The drawn cards, one per month. The spread runs from the month after
/// the month the reading is created in.</param>
/// <param name="UserId">The authenticated user's ID.</param>
public record CreateTwelveMonthsReadingCommand(
    string Locale,
    List<AiDeepCardRequest> Cards,
    Guid UserId
) : IRequest<Guid>;
