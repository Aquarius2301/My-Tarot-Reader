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
        var readingCount = await context
            .AITarotReadingCounts.IgnoreQueryFilters()
            .FirstOrDefaultAsync(s => s.CardCount == request.CardCount, cancellationToken);

        if (readingCount is { DeletedAt: null })
        {
            throw new ConflictException(AiTarotErrorCode.CardCountExists);
        }

        if (readingCount is null)
        {
            readingCount = new AITarotReadingCount();
            context.AITarotReadingCounts.Add(readingCount);
        }

        readingCount.CardCount = request.CardCount;
        readingCount.WhiteCoins = request.WhiteCoins;
        readingCount.PositionsEn = request.PositionsEn;
        readingCount.PositionsVi = request.PositionsVi;
        readingCount.MinWords = request.MinWords;
        readingCount.MaxWords = request.MaxWords;
        readingCount.IsEnabled = request.IsEnabled;
        readingCount.DeletedAt = null;

        await context.SaveChangesAsync(cancellationToken);

        return readingCount.Id;
    }
}
