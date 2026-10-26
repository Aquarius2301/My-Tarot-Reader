using MediatR;
using Microsoft.EntityFrameworkCore;
using MyTarotReader.Application.Common.Exceptions;
using MyTarotReader.Application.Constants.Errors;
using MyTarotReader.Application.Persistence;
using MyTarotReader.Domain.Entities;

namespace MyTarotReader.Application.Features.AiTarot;

public class CreateAiTarotReadingCountCommandHandler(IAppDbContext context)
    : IRequestHandler<CreateAiTarotReadingCountCommand, Guid>
{
    public async Task<Guid> Handle(
        CreateAiTarotReadingCountCommand request,
        CancellationToken cancellationToken
    )
    {
        var cardCountExists = await context
            .AITarotReadingCounts.IgnoreQueryFilters()
            .AnyAsync(s => s.CardCount == request.CardCount, cancellationToken);

        if (cardCountExists)
        {
            throw new ConflictException(AiTarotErrorCode.CardCountExists);
        }

        var readingCount = new AITarotReadingCount
        {
            CardCount = request.CardCount,
            WhiteCoins = request.WhiteCoins,
            PositionsEn = request.PositionsEn,
            PositionsVi = request.PositionsVi,
            MinWords = request.MinWords,
            MaxWords = request.MaxWords,
            IsEnabled = request.IsEnabled,
        };

        context.AITarotReadingCounts.Add(readingCount);
        await context.SaveChangesAsync(cancellationToken);

        return readingCount.Id;
    }
}
