using FluentValidation;
using MyTarotReader.Application.Constants.Errors;
using MyTarotReader.Application.Contracts.Services;

namespace MyTarotReader.Application.Common.Validators;

public class AddCoinRequestValidator : AbstractValidator<AddCoinRequest>
{
    public AddCoinRequestValidator()
    {
        RuleFor(x => x.WhiteCoins)
            .GreaterThanOrEqualTo(0)
            .WithMessage(WalletErrorCode.InvalidAmount);

        RuleFor(x => x.RedCoins)
            .GreaterThanOrEqualTo(0)
            .WithMessage(WalletErrorCode.InvalidAmount);

        RuleFor(x => x)
            .Must(x => x.WhiteCoins + x.RedCoins > 0)
            .WithMessage(WalletErrorCode.InvalidAmount)
            .WithName(nameof(AddCoinRequest.WhiteCoins));

        RuleFor(x => x.Type).IsInEnum().WithMessage(WalletErrorCode.InvalidAmount);
    }
}

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