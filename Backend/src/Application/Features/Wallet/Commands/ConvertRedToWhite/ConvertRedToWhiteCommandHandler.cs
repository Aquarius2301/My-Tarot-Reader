using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MyTarotReader.Application.Common.Exceptions;
using MyTarotReader.Application.Constants.Errors;
using MyTarotReader.Application.Persistence;
using MyTarotReader.Application.Settings;
using MyTarotReader.Domain.Entities;
using MyTarotReader.Domain.Enums;

namespace MyTarotReader.Application.Features.Wallet;

public class ConvertRedToWhiteCommandHandler(
    IAppDbContext context,
    IOptions<WalletSetting> walletSetting
) : IRequestHandler<ConvertRedToWhiteCommand, Unit>
{
    private readonly WalletSetting _walletSetting = walletSetting.Value;

    public async Task<Unit> Handle(
        ConvertRedToWhiteCommand request,
        CancellationToken cancellationToken
    )
    {
        var wallet =
            await context
                .Wallets.Include(w => w.WhiteCoinBatches)
                .FirstOrDefaultAsync(w => w.UserId == request.UserId, cancellationToken)
            ?? throw new NotFoundException(WalletErrorCode.WalletNotFound);

        if (wallet.RedCoin < request.RedCoins)
        {
            throw new BadRequestException(WalletErrorCode.InsufficientRedCoins);
        }

        var whiteCoins = request.RedCoins * 2;

        await using var transaction = await context.Database.BeginTransactionAsync(
            cancellationToken
        );

        wallet.RedCoin -= request.RedCoins;

        var whiteBatch = new WhiteCoinBatch
        {
            WalletId = wallet.Id,
            Amount = whiteCoins,
            RemainingAmount = whiteCoins,
            ExpiredAt = DateTimeOffset.UtcNow.AddDays(_walletSetting.ExpireDays),
        };
        context.WhiteCoinBatches.Add(whiteBatch);

        context.Orders.Add(
            new Order
            {
                UserId = request.UserId,
                RedCoins = 0,
                WhiteCoins = whiteCoins,
                Description = $"Convert {request.RedCoins} red coins to {whiteCoins} white coins",
                Type = OrderType.RedToWhite,
            }
        );

        wallet.UpdatedAt = DateTimeOffset.UtcNow;

        await context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return Unit.Value;
    }
}
