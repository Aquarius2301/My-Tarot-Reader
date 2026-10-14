using MediatR;
using Microsoft.EntityFrameworkCore;
using MyTarotReader.Application.Persistence;

namespace MyTarotReader.Application.Features.Auth;

public class LogoutCommandHandler(IAppDbContext context) : IRequestHandler<LogoutCommand, Unit>
{
    public async Task<Unit> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        var stored = await context.RefreshTokens.FirstOrDefaultAsync(
            r => r.Token == request.RefreshToken,
            cancellationToken
        );

        if (stored != null)
        {
            stored.DeletedAt = DateTimeOffset.UtcNow;
            await context.SaveChangesAsync(cancellationToken);
        }

        return Unit.Value;
    }
}
