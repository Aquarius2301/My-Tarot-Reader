using MediatR;
using Microsoft.EntityFrameworkCore;
using MyTarotReader.Application.Common.Exceptions;
using MyTarotReader.Application.Constants.Errors;
using MyTarotReader.Application.Persistence;

namespace MyTarotReader.Application.Features.Tarot;

public class RemoveDrawnForAuthCommandHandler(IAppDbContext context)
    : IRequestHandler<RemoveDrawnForAuthCommand>
{
    public async Task Handle(
        RemoveDrawnForAuthCommand request,
        CancellationToken cancellationToken = default
    )
    {
        var record =
            await context.TarotReadings.FirstOrDefaultAsync(
                r => r.Id == request.ReadingId && r.UserId == request.UserId,
                cancellationToken
            ) ?? throw new NotFoundException(TarotReadingErrorCode.NotFound);

        record.DeletedAt = DateTimeOffset.UtcNow;
        await context.SaveChangesAsync(cancellationToken);
    }
}