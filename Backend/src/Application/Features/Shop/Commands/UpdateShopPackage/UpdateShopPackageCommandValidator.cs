using FluentValidation;
using MyTarotReader.Application.Constants.Errors;

namespace MyTarotReader.Application.Features.Shop;

public class UpdateShopPackageCommandValidator : AbstractValidator<UpdateShopPackageCommand>
{
    public UpdateShopPackageCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage(ShopErrorCode.PackageNotFound);

        RuleFor(x => x.Code)
            .NotEmpty()
            .WithMessage(ShopErrorCode.InvalidPackage)
            .MaximumLength(50)
            .WithMessage(ShopErrorCode.InvalidPackage);

        RuleFor(x => x.RedCoins).GreaterThanOrEqualTo(0).WithMessage(ShopErrorCode.InvalidPackage);
        RuleFor(x => x.WhiteCoins).GreaterThanOrEqualTo(0).WithMessage(ShopErrorCode.InvalidPackage);
        RuleFor(x => x.OldPrice).GreaterThanOrEqualTo(0).WithMessage(ShopErrorCode.InvalidPackage);
        RuleFor(x => x.Price).GreaterThan(0).WithMessage(ShopErrorCode.InvalidPackage);
    }
}
