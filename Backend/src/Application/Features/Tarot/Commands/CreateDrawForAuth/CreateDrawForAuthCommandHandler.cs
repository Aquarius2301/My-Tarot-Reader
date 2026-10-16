using MediatR;
using MyTarotReader.Application.Persistence;
using MyTarotReader.Domain.Entities;

namespace MyTarotReader.Application.Features.Tarot;

public class CreateDrawForAuthCommandHandler(IAppDbContext context)
    : IRequestHandler<CreateDrawForAuthCommand>
{
    public async Task Handle(CreateDrawForAuthCommand request, CancellationToken cancellationToken)
    {
        var entity = new TarotReading
        {
            CardCode = request.CardCode,
            IsReversed = request.IsReversed,
            UserId = request.UserId,
        };

        context.TarotReadings.Add(entity);

        await context.SaveChangesAsync(cancellationToken);
    }
}