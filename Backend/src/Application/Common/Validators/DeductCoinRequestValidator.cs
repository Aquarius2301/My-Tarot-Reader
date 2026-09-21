using FluentValidation;
using MyTarotReader.Application.Constants.Errors;
using MyTarotReader.Application.Contracts.Services;

namespace MyTarotReader.Application.Common.Validators;

public class DeductCoinRequestValidator : AbstractValidator<DeductCoinRequest>
{
    public DeductCoinRequestValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage(WalletErrorCode.InvalidAmount);

        RuleFor(x => x.Type).IsInEnum().WithMessage(WalletErrorCode.InvalidAmount);
    }
}