using MediatR;
using Microsoft.EntityFrameworkCore;
using MyTarotReader.Application.Common.Exceptions;
using MyTarotReader.Application.Constants.Errors;
using MyTarotReader.Application.Persistence;

namespace MyTarotReader.Application.Features.AiDeepTarot;

public class DeleteAiDeepTarotReadingCommandHandler(IAppDbContext context)
    : IRequestHandler<DeleteAiDeepTarotReadingCommand, Unit>
{
    public async Task<Unit> Handle(
        DeleteAiDeepTarotReadingCommand request,
        CancellationToken cancellationToken = default
    )
    {
        var reading =
            await context.AIDeepTarotReadings.FirstOrDefaultAsync(
                r => r.Id == request.ReadingId && r.UserId == request.UserId,
                cancellationToken
            ) ?? throw new NotFoundException(AiDeepTarotErrorCode.ReadingNotFound);

        reading.DeletedAt = DateTimeOffset.UtcNow;
        await context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
