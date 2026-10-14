using FluentValidation;
using MyTarotReader.Application.Constants.Errors;

namespace MyTarotReader.Application.Features.Wallet;

public class ConvertRedToWhiteCommandValidator : AbstractValidator<ConvertRedToWhiteCommand>
{
    public ConvertRedToWhiteCommandValidator()
    {
        RuleFor(x => x.RedCoins)
            .GreaterThan(0)
            .WithMessage(WalletErrorCode.InvalidAmount);
    }
}
