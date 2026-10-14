using MediatR;
using Microsoft.EntityFrameworkCore;
using MyTarotReader.Application.Common.Exceptions;
using MyTarotReader.Application.Persistence;

namespace MyTarotReader.Application.Features.Auth;

public class GetCurrentUserHandler(IAppDbContext context)
    : IRequestHandler<GetCurrentUserQuery, GetCurrentUserResult>
{
    public async Task<GetCurrentUserResult> Handle(
        GetCurrentUserQuery request,
        CancellationToken cancellationToken
    )
    {
        var user =
            await context
                .Users.AsNoTracking()
                .Where(u => u.Id == request.UserId)
                .Select(x => new GetCurrentUserResult(
                    x.Id,
                    x.FullName,
                    x.Email,
                    x.Picture,
                    x.Wallet.WhiteCoinBatches.Where(b =>
                            b.RemainingAmount > 0 && b.ExpiredAt >= DateTimeOffset.UtcNow
                        )
                        .Sum(b => b.RemainingAmount),
                    x.Wallet.RedCoin,
                    x.Role
                ))
                .FirstOrDefaultAsync(cancellationToken)
            ?? throw new UnauthorizedException();

        return user;
    }
}
