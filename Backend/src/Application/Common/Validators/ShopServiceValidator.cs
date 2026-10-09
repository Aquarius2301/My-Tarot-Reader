using FluentValidation;
using MyTarotReader.Application.Constants.Errors;
using MyTarotReader.Application.Contracts.Services;

namespace MyTarotReader.Application.Common.Validators;

public class CreatePaymentRequestValidator : AbstractValidator<CreatePaymentRequest>
{
    public CreatePaymentRequestValidator()
    {
        RuleFor(x => x.PackageCode)
            .NotEmpty()
            .WithMessage(ShopErrorCode.InvalidPackage)
            .MaximumLength(50)
            .WithMessage(ShopErrorCode.InvalidPackage);
    }
}
