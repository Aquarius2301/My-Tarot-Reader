using System;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MyTarotReader.Application.Common.Exceptions;
using MyTarotReader.Application.Constants.Errors;
using MyTarotReader.Application.Persistence;

namespace MyTarotReader.Application.Features.Wallet;

public class GetWalletQueryHandler(IAppDbContext context)
    : IRequestHandler<GetWalletQuery, GetWalletResult>
{
    public async Task<GetWalletResult> Handle(
        GetWalletQuery request,
        CancellationToken cancellationToken = default
    )
    {
        var now = DateTimeOffset.UtcNow;

        var wallet =
            await context
                .Wallets.AsNoTracking()
                .Where(w => w.UserId == request.UserId)
                .Select(w => new GetWalletResult(
                    w.WhiteCoinBatches.Where(b => b.RemainingAmount > 0 && b.ExpiredAt >= now)
                        .Sum(b => b.RemainingAmount),
                    w.RedCoin,
                    w.WhiteCoinBatches.Where(b => b.RemainingAmount > 0 && b.ExpiredAt >= now)
                        .OrderBy(b => b.ExpiredAt)
                        .Select(b => new WhiteCoinBatchItem(
                            b.Id,
                            b.Amount,
                            b.RemainingAmount,
                            b.ExpiredAt
                        ))
                        .ToList()
                ))
                .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException(WalletErrorCode.WalletNotFound);

        return wallet;
    }
}
