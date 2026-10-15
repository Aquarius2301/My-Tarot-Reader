using MediatR;
using Microsoft.EntityFrameworkCore;
using MyTarotReader.Application.Common.Exceptions;
using MyTarotReader.Application.Constants.Errors;
using MyTarotReader.Application.Persistence;

namespace MyTarotReader.Application.Features.AiTarot;

public class DeleteAiTarotReadingCommandHandler(IAppDbContext context)
    : IRequestHandler<DeleteAiTarotReadingCommand, Unit>
{
    public async Task<Unit> Handle(
        DeleteAiTarotReadingCommand request,
        CancellationToken cancellationToken
    )
    {
        var reading =
            await context.AITarotReadings.FirstOrDefaultAsync(
                r => r.Id == request.ReadingId && r.UserId == request.UserId,
                cancellationToken
            ) ?? throw new NotFoundException(AiTarotErrorCode.ReadingNotFound);

        reading.DeletedAt = DateTimeOffset.UtcNow;
        await context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
