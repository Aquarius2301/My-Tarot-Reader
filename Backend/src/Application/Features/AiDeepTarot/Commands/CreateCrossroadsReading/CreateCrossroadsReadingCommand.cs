using MediatR;
using MyTarotReader.Domain.Enums;

namespace MyTarotReader.Application.Features.AiDeepTarot;

/// <summary>
/// Request to create a crossroads (decision between options) deep tarot reading.
/// </summary>
/// <param name="Locale">The response language of the reading ("en" or "vi").</param>
/// <param name="Question">The decision the user is facing, in their own words.</param>
/// <param name="Options">The options being compared, between 2 and 4 of them.</param>
/// <param name="TimeFrame">The horizon over which the decision is evaluated, optional.</param>
/// <param name="Cards">The drawn cards: three per option, plus one closing summary card.</param>
/// <param name="UserId">The authenticated user's ID.</param>
public record CreateCrossroadsReadingCommand(
    string Locale,
    string Question,
    List<string> Options,
    CrossroadsTimeFrame? TimeFrame,
    List<AiDeepCardRequest> Cards,
    Guid UserId
) : IRequest<Guid>;
