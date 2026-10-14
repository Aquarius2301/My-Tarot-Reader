using MediatR;
using Microsoft.EntityFrameworkCore;
using MyTarotReader.Application.Common.Exceptions;
using MyTarotReader.Application.Constants.Errors;
using MyTarotReader.Application.Constants.Tarot;
using MyTarotReader.Application.Persistence;

namespace MyTarotReader.Application.Features.AiDeepTarot;

public class GetAiDeepTarotReadingByIdQueryHandler(IAppDbContext context)
    : IRequestHandler<GetAiDeepTarotReadingByIdQuery, GetAiDeepTarotReadingResult>
{
    public async Task<GetAiDeepTarotReadingResult> Handle(
        GetAiDeepTarotReadingByIdQuery request,
        CancellationToken cancellationToken = default
    )
    {
        var reading =
            await context
                .AIDeepTarotReadings.AsNoTracking()
                .Where(r => r.Id == request.ReadingId && r.UserId == request.UserId)
                .Select(r => new
                {
                    r.Id,
                    r.Topic,
                    r.Title,
                    r.Answer,
                    r.Cards,
                    r.Question,
                    r.Options,
                    r.TimeFrame,
                    r.CreatedAt,
                })
                .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException(AiDeepTarotErrorCode.ReadingNotFound);

        var cards = AiDeepTarotHelper.DeserializeCards(reading.Cards);
        var options = AiDeepTarotHelper.DeserializeOptions(reading.Options);

        var answerJson = reading.Answer;
        if (
            AiDeepTarotHelper.TryParseAnswerJson(answerJson) is { } parsedAnswer
            && DeepTarotConstant.IsSupported(reading.Topic)
        )
        {
            answerJson = AiDeepTarotHelper.NormalizeAnswer(
                parsedAnswer,
                DeepTarotConstant.GetPositions(reading.Topic, options.Count),
                [.. cards.Select(c => c.CardCode)]
            );
        }

        return new GetAiDeepTarotReadingResult(
            reading.Id,
            reading.Topic,
            reading.Title,
            answerJson,
            cards,
            reading.CreatedAt,
            reading.Question,
            options.Count > 0 ? options : null,
            reading.TimeFrame
        );
    }
}
