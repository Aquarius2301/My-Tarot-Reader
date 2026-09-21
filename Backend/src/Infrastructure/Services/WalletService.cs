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
    IValidator<AddCoinRequest> addCoinValidator,
    IValidator<DeductCoinRequest> deductCoinValidator
) : IWalletService
{
    private readonly IAppDbContext _context = context;
    private readonly WalletSetting _walletSetting = walletSetting.Value;
    private readonly IValidator<AddCoinRequest> _addCoinValidator = addCoinValidator;
    private readonly IValidator<DeductCoinRequest> _deductCoinValidator = deductCoinValidator;

    /// <summary>
    /// Adds white and/or red coins to a user's wallet and records the transaction as an order.
    /// </summary>
    /// <param name="userId">The authenticated user's ID.</param>
    /// <param name="request"><see cref="AddCoinRequest"/> containing the coin amounts and order type.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <exception cref="BadRequestException">Thrown when the request validation fails (amounts invalid).</exception>
    /// <exception cref="NotFoundException">Thrown when no wallet exists for the user.</exception>
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

        var whiteCoin = wallet
            .WhiteCoinBatches.Where(b =>
                b.RemainingAmount > 0 && b.ExpiredAt >= DateTimeOffset.UtcNow
            )
            .Sum(b => b.RemainingAmount);

        return new AddCoinResult(wallet.RedCoin, whiteCoin);
    }

    /// <summary>
    /// Returns the current white and red coin balances for a user.
    /// </summary>
    /// <param name="userId">The authenticated user's ID.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <exception cref="NotFoundException">Thrown when no wallet exists for the user.</exception>
    /// <returns><see cref="GetWalletBalanceResult"/> with the current balances.</returns>
    public async Task<GetWalletBalanceResult> GetBalanceAsync(
        Guid userId,
        CancellationToken cancellationToken = default
    )
    {
        var now = DateTimeOffset.UtcNow;

        var balance =
            await _context
                .Wallets.AsNoTracking()
                .Where(w => w.UserId == userId)
                .Select(w => new GetWalletBalanceResult(
                    w.WhiteCoinBatches.Where(b => b.RemainingAmount > 0 && b.ExpiredAt >= now)
                        .Sum(b => b.RemainingAmount),
                    w.RedCoin
                ))
                .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException(WalletErrorCode.WalletNotFound);

        return balance;
    }

    /// <summary>
    /// Deducts white coins from a user's wallet, spending the oldest active batch first,
    /// and records the transaction as an order.
    /// </summary>
    /// <param name="userId">The authenticated user's ID.</param>
    /// <param name="request"><see cref="DeductCoinRequest"/> containing the amount and order type.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <exception cref="BadRequestException">Thrown when the request validation fails or the user does not have enough white coins.</exception>
    /// <exception cref="NotFoundException">Thrown when no wallet exists for the user.</exception>
    /// <returns><see cref="DeductCoinResult"/> with the updated white coin balance.</returns>
    public async Task<DeductCoinResult> DeductCoinAsync(
        Guid userId,
        DeductCoinRequest request,
        CancellationToken cancellationToken = default
    )
    {
        ValidationHelper.ValidateOrThrow(_deductCoinValidator, request);

        var wallet =
            await _context
                .Wallets.Include(w => w.WhiteCoinBatches)
                .FirstOrDefaultAsync(w => w.UserId == userId, cancellationToken)
            ?? throw new NotFoundException(WalletErrorCode.WalletNotFound);

        var now = DateTimeOffset.UtcNow;
        var activeBatches = wallet
            .WhiteCoinBatches.Where(b => b.RemainingAmount > 0 && b.ExpiredAt >= now)
            .OrderBy(b => b.CreatedAt)
            .ToList();

        var available = activeBatches.Sum(b => b.RemainingAmount);
        if (available < request.Amount)
        {
            throw new BadRequestException(WalletErrorCode.InsufficientCoins);
        }

        var remainingToDeduct = request.Amount;
        var orderDetails = new List<OrderDetail>();
        foreach (var batch in activeBatches)
        {
            if (remainingToDeduct <= 0)
            {
                break;
            }

            var take = Math.Min(batch.RemainingAmount, remainingToDeduct);
            batch.RemainingAmount -= take;
            remainingToDeduct -= take;
            orderDetails.Add(new OrderDetail { WhiteCoinBatchId = batch.Id, Amount = take });
        }

        _context.Orders.Add(
            new Order
            {
                UserId = userId,
                Amount = request.Amount,
                Description = $"Spend coins ({request.Type})",
                Type = request.Type,
                OrderDetails = orderDetails,
            }
        );

        wallet.UpdatedAt = DateTimeOffset.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return new DeductCoinResult(activeBatches.Sum(b => b.RemainingAmount));
    }
}
