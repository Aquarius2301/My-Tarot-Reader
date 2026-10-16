using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MyTarotReader.Application.Common.Exceptions;
using MyTarotReader.Application.Constants.Errors;
using MyTarotReader.Application.Persistence;

namespace MyTarotReader.Application.Features.AiTarot;

public class GetAiTarotReadingByIdQueryHandler(IAppDbContext _context)
    : IRequestHandler<GetAiTarotReadingByIdQuery, GetAiTarotReadingResult>
{
    public async Task<GetAiTarotReadingResult> Handle(
        GetAiTarotReadingByIdQuery request,
        CancellationToken cancellationToken = default
    )
    {
        var reading =
            await _context
                .AITarotReadings.AsNoTracking()
                .Where(r => r.Id == request.ReadingId && r.UserId == request.UserId)
                .Select(r => new
                {
                    r.Id,
                    r.CardCount,
                    r.QuestionType,
                    r.Title,
                    r.Answer,
                    r.Cards,
                    r.CreatedAt,
                })
                .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException(AiTarotErrorCode.ReadingNotFound);

        var cards = AiTarotHelper.DeserializeCards(reading.Cards);
        Console.WriteLine(
            "card"
                + JsonSerializer.Serialize(
                    cards,
                    new JsonSerializerOptions { WriteIndented = true }
                )
        );
        var answerJson = reading.Answer;

        if (AiTarotHelper.TryParseAnswerJson(answerJson) is { } parsedAnswer)
        {
            answerJson = AiTarotHelper.NormalizeAnswer(
                parsedAnswer,
                [.. cards.Select(c => c.CardCode)]
            );
        }

        return new GetAiTarotReadingResult(
            reading.Id,
            reading.CardCount,
            reading.QuestionType,
            reading.Title,
            answerJson,
            cards,
            reading.CreatedAt
        );
    }
}
