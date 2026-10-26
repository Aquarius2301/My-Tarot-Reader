using MediatR;
using Microsoft.EntityFrameworkCore;
using MyTarotReader.Application.Common.Exceptions;
using MyTarotReader.Application.Constants.Errors;
using MyTarotReader.Application.Persistence;

namespace MyTarotReader.Application.Features.Shop;

public class DeleteShopPackageCommandHandler(IAppDbContext context)
    : IRequestHandler<DeleteShopPackageCommand, Unit>
{
    public async Task<Unit> Handle(
        DeleteShopPackageCommand request,
        CancellationToken cancellationToken
    )
    {
        var package =
            await context.ShopPackages.FirstOrDefaultAsync(
                p => p.Id == request.Id,
                cancellationToken
            ) ?? throw new NotFoundException(ShopErrorCode.PackageNotFound);

        package.DeletedAt = DateTimeOffset.UtcNow;
        await context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
