using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MyTarotReader.Application.Common.Exceptions;
using MyTarotReader.Application.Common.Validators;
using MyTarotReader.Application.Constants.Errors;
using MyTarotReader.Application.Contracts.Persistence;
using MyTarotReader.Application.Contracts.Services;
using MyTarotReader.Application.Settings;
using MyTarotReader.Domain.Entities;

namespace MyTarotReader.Infrastructure.Services;

public class WalletService(
    IAppDbContext context,
    IOptions<WalletSetting> walletSetting,
    IValidator<AddCoinRequest> addCoinValidator
) : IWalletService
{
    private readonly IAppDbContext _context = context;
    private readonly WalletSetting _walletSetting = walletSetting.Value;
    private readonly IValidator<AddCoinRequest> _addCoinValidator = addCoinValidator;

    public async Task<AddCoinResult> AddCoinAsync(
        Guid userId,
        AddCoinRequest request,
        CancellationToken cancellationToken = default
    )
    {
        ValidationHelper.ValidateOrThrow(_addCoinValidator, request);

        var wallet =
            await _context
                .Wallets.Include(w => w.WhiteCoinBatches)
                .FirstOrDefaultAsync(w => w.UserId == userId, cancellationToken)
            ?? throw new NotFoundException(WalletErrorCode.WalletNotFound);

        WhiteCoinBatch? whiteBatch = null;
        if (request.WhiteCoins > 0)
        {
            whiteBatch = new WhiteCoinBatch
            {
                WalletId = wallet.Id,
                Amount = request.WhiteCoins,
                RemainingAmount = request.WhiteCoins,
                ExpiredAt = DateTimeOffset.UtcNow.AddDays(_walletSetting.ExpireDays),
            };
            _context.WhiteCoinBatches.Add(whiteBatch);
        }

        if (request.RedCoins > 0)
        {
            wallet.RedCoin += request.RedCoins;
        }

        var orderDetails = new List<OrderDetail>();
        if (whiteBatch is not null)
        {
            orderDetails.Add(
                new OrderDetail { WhiteCoinBatchId = whiteBatch.Id, Amount = request.WhiteCoins }
            );
        }
        if (request.RedCoins > 0)
        {
            orderDetails.Add(new OrderDetail { Amount = request.RedCoins });
        }

        var order = new Order
        {
            UserId = userId,
            Amount = request.WhiteCoins + request.RedCoins,
            Description = $"Top-up coins ({request.Type})",
            Type = request.Type,
            OrderDetails = orderDetails,
        };
        _context.Orders.Add(order);

        wallet.UpdatedAt = DateTimeOffset.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        var whiteCoin = wallet.WhiteCoinBatches
            .Where(b => b.RemainingAmount > 0 && b.ExpiredAt >= DateTimeOffset.UtcNow)
            .Sum(b => b.RemainingAmount);

        return new AddCoinResult(wallet.RedCoin, whiteCoin);
    }
}