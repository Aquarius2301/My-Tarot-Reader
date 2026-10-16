using FluentValidation;
using MyTarotReader.Application.Constants.Errors;
using MyTarotReader.Application.Constants.Tarot;

namespace MyTarotReader.Application.Features.Tarot;

public class CreateDrawForGuestCommandValidator : AbstractValidator<CreateDrawForGuestCommand>
{
    public CreateDrawForGuestCommandValidator()
    {
        RuleFor(x => x.CardCode)
            .NotEmpty()
            .WithMessage(TarotReadingErrorCode.InvalidCardCode)
            .Must(TarotConstant.IsValidCardCode)
            .WithMessage(TarotReadingErrorCode.InvalidCardCode);

        RuleFor(x => x.GuestKey).NotEmpty().WithMessage(TarotReadingErrorCode.InvalidGuestKey);
    }
}
