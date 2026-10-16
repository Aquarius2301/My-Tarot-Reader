using MediatR;

namespace MyTarotReader.Application.Features.AiDeepTarot;

/// <summary>
/// Request to create a 12 astrological houses deep tarot reading.
/// </summary>
/// <param name="Locale">The response language of the reading ("en" or "vi").</param>
/// <param name="Cards">The drawn cards, one per house.</param>
/// <param name="UserId">The authenticated user's ID.</param>
public record CreateTwelveHousesReadingCommand(
    string Locale,
    List<AiDeepCardRequest> Cards,
    Guid UserId
) : IRequest<Guid>;
