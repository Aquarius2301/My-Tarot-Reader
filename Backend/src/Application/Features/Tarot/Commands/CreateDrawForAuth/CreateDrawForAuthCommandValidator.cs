using FluentValidation;
using MyTarotReader.Application.Constants.Errors;
using MyTarotReader.Application.Constants.Tarot;

namespace MyTarotReader.Application.Features.Tarot;

public class CreateDrawForAuthCommandValidator : AbstractValidator<CreateDrawForAuthCommand>
{
    public CreateDrawForAuthCommandValidator()
    {
        RuleFor(x => x.CardCode)
            .Must(TarotConstant.IsValidCardCode)
            .WithMessage(TarotReadingErrorCode.InvalidCardCode);
    }
}
