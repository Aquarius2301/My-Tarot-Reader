using MediatR;
using Microsoft.EntityFrameworkCore;
using MyTarotReader.Application.Common.Exceptions;
using MyTarotReader.Application.Constants.Errors;
using MyTarotReader.Application.Persistence;

namespace MyTarotReader.Application.Features.AiTarot;

public class UpdateAiTarotReadingCountCommandHandler(IAppDbContext context)
    : IRequestHandler<UpdateAiTarotReadingCountCommand, Unit>
{
    public async Task<Unit> Handle(
        UpdateAiTarotReadingCountCommand request,
        CancellationToken cancellationToken
    )
    {
        var readingCount =
            await context.AITarotReadingCounts.FirstOrDefaultAsync(
                s => s.Id == request.Id,
                cancellationToken
            ) ?? throw new NotFoundException(AiTarotErrorCode.ReadingCountNotFound);

        var cardCountExists = await context
            .AITarotReadingCounts.IgnoreQueryFilters()
            .AnyAsync(
                s => s.CardCount == request.CardCount && s.Id != request.Id,
                cancellationToken
            );

        if (cardCountExists)
        {
            throw new ConflictException(AiTarotErrorCode.CardCountExists);
        }

        readingCount.CardCount = request.CardCount;
        readingCount.WhiteCoins = request.WhiteCoins;
        readingCount.PositionsEn = request.PositionsEn;
        readingCount.PositionsVi = request.PositionsVi;
        readingCount.MinWords = request.MinWords;
        readingCount.MaxWords = request.MaxWords;
        readingCount.IsEnabled = request.IsEnabled;

        await context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
