using System;
using FluentValidation;
using MyTarotReader.Application.Constants.Errors;

namespace MyTarotReader.Application.Features.Shop;

public class CreatePaymentCommandValidator : AbstractValidator<CreatePaymentCommand>
{
    public CreatePaymentCommandValidator()
    {
        RuleFor(x => x.PackageCode)
            .NotEmpty()
            .WithMessage(ShopErrorCode.InvalidPackage)
            .MaximumLength(50)
            .WithMessage(ShopErrorCode.InvalidPackage);
    }
}
