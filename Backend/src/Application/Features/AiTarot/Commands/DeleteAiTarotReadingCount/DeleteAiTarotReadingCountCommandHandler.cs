using MediatR;
using Microsoft.EntityFrameworkCore;
using MyTarotReader.Application.Common.Exceptions;
using MyTarotReader.Application.Constants.Errors;
using MyTarotReader.Application.Persistence;

namespace MyTarotReader.Application.Features.AiTarot;

public class DeleteAiTarotReadingCountCommandHandler(IAppDbContext context)
    : IRequestHandler<DeleteAiTarotReadingCountCommand, Unit>
{
    public async Task<Unit> Handle(
        DeleteAiTarotReadingCountCommand request,
        CancellationToken cancellationToken
    )
    {
        var readingCount =
            await context.AITarotReadingCounts.FirstOrDefaultAsync(
                s => s.Id == request.Id,
                cancellationToken
            ) ?? throw new NotFoundException(AiTarotErrorCode.ReadingCountNotFound);

        readingCount.DeletedAt = DateTimeOffset.UtcNow;
        await context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
