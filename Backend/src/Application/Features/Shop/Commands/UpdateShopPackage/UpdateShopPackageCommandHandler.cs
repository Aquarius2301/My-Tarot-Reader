using MediatR;
using Microsoft.EntityFrameworkCore;
using MyTarotReader.Application.Common.Exceptions;
using MyTarotReader.Application.Constants.Errors;
using MyTarotReader.Application.Persistence;

namespace MyTarotReader.Application.Features.Shop;

public class UpdateShopPackageCommandHandler(IAppDbContext context)
    : IRequestHandler<UpdateShopPackageCommand, Unit>
{
    public async Task<Unit> Handle(
        UpdateShopPackageCommand request,
        CancellationToken cancellationToken
    )
    {
        var package =
            await context.ShopPackages.FirstOrDefaultAsync(
                p => p.Id == request.Id,
                cancellationToken
            ) ?? throw new NotFoundException(ShopErrorCode.PackageNotFound);

        var code = request.Code.Trim().ToLowerInvariant();

        var codeExists = await context
            .ShopPackages.IgnoreQueryFilters()
            .AnyAsync(
                p => p.Code.ToLower() == code && p.Id != request.Id,
                cancellationToken
            );

        if (codeExists)
        {
            throw new ConflictException(ShopErrorCode.PackageCodeExists);
        }

        package.Code = code;
        package.RedCoins = request.RedCoins;
        package.WhiteCoins = request.WhiteCoins;
        package.OldPrice = request.OldPrice;
        package.Price = request.Price;
        package.IsEnabled = request.IsEnabled;

        await context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
