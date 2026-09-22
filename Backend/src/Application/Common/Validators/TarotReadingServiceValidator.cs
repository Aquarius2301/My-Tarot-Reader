using FluentValidation;
using MyTarotReader.Application.Constants.Errors;
using MyTarotReader.Application.Constants.Tarot;
using MyTarotReader.Application.Contracts.Services;

namespace MyTarotReader.Application.Common.Validators;

public class CreateDrawForAuthRequestValidator : AbstractValidator<CreateDrawForAuthRequest>
{
    public CreateDrawForAuthRequestValidator()
    {
        RuleFor(x => x.CardCode)
            .NotEmpty()
            .WithMessage(TarotReadingErrorCode.InvalidCardCode)
            .Must(TarotConstant.IsValidCardCode)
            .WithMessage(TarotReadingErrorCode.InvalidCardCode);
    }
}

public class CreateDrawForGuestRequestValidator : AbstractValidator<CreateDrawForGuestRequest>
{
    public CreateDrawForGuestRequestValidator()
    {
        RuleFor(x => x.CardCode)
            .NotEmpty()
            .WithMessage(TarotReadingErrorCode.InvalidCardCode)
            .Must(TarotConstant.IsValidCardCode)
            .WithMessage(TarotReadingErrorCode.InvalidCardCode);
    }
}
