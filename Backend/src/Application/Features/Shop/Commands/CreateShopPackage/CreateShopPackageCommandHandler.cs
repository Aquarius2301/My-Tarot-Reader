using MediatR;
using Microsoft.EntityFrameworkCore;
using MyTarotReader.Application.Common.Exceptions;
using MyTarotReader.Application.Constants.Errors;
using MyTarotReader.Application.Persistence;
using MyTarotReader.Domain.Entities;

namespace MyTarotReader.Application.Features.Shop;

public class CreateShopPackageCommandHandler(IAppDbContext context)
    : IRequestHandler<CreateShopPackageCommand, Guid>
{
    public async Task<Guid> Handle(
        CreateShopPackageCommand request,
        CancellationToken cancellationToken
    )
    {
        var code = request.Code.Trim().ToLowerInvariant();

        var package = await context
            .ShopPackages.IgnoreQueryFilters()
            .FirstOrDefaultAsync(p => p.Code.ToLower() == code, cancellationToken);

        if (package is { DeletedAt: null })
        {
            throw new ConflictException(ShopErrorCode.PackageCodeExists);
        }

        if (package is null)
        {
            package = new ShopPackage();
            context.ShopPackages.Add(package);
        }

        package.Code = code;
        package.RedCoins = request.RedCoins;
        package.WhiteCoins = request.WhiteCoins;
        package.OldPrice = request.OldPrice;
        package.Price = request.Price;
        package.IsEnabled = request.IsEnabled;
        package.DeletedAt = null;

        await context.SaveChangesAsync(cancellationToken);

        return package.Id;
    }
}
