using System;
using MyTarotReader.Domain.Enums;

namespace MyTarotReader.Application.Services;

public record UserCoins(int RedCoins, int WhiteCoins);

public interface IWalletService
{
    Task<UserCoins> GetCoinsAsync(Guid userId, CancellationToken cancellationToken);

    Task AddCoinsAsync(
        Guid userId,
        int redCoins,
        int whiteCoins,
        OrderType orderType,
        CancellationToken cancellationToken
    );

    Task DeductCoinsAsync(
        Guid userId,
        int redCoins,
        int whiteCoins,
        OrderType orderType,
        CancellationToken cancellationToken
    );
}
