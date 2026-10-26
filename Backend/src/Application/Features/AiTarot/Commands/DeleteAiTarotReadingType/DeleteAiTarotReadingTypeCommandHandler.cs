using MediatR;
using Microsoft.EntityFrameworkCore;
using MyTarotReader.Application.Common.Exceptions;
using MyTarotReader.Application.Constants.Errors;
using MyTarotReader.Application.Persistence;

namespace MyTarotReader.Application.Features.AiTarot;

public class DeleteAiTarotReadingTypeCommandHandler(IAppDbContext context)
    : IRequestHandler<DeleteAiTarotReadingTypeCommand, Unit>
{
    public async Task<Unit> Handle(
        DeleteAiTarotReadingTypeCommand request,
        CancellationToken cancellationToken
    )
    {
        var readingType =
            await context.AITarotReadingTypes.FirstOrDefaultAsync(
                s => s.Id == request.Id,
                cancellationToken
            ) ?? throw new NotFoundException(AiTarotErrorCode.ReadingTypeNotFound);

        readingType.DeletedAt = DateTimeOffset.UtcNow;
        await context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
