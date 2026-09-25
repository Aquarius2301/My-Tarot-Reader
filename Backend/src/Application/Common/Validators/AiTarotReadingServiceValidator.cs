using FluentValidation;
using MyTarotReader.Application.Constants.Errors;
using MyTarotReader.Application.Constants.Tarot;
using MyTarotReader.Application.Contracts.Services;

namespace MyTarotReader.Application.Common.Validators;

public class CreateAiTarotReadingRequestValidator : AbstractValidator<CreateAiTarotReadingRequest>
{
    public CreateAiTarotReadingRequestValidator()
    {
        RuleFor(x => x.CardCount).IsInEnum().WithMessage(AiTarotErrorCode.InvalidCardCount);

        RuleFor(x => x.Type).IsInEnum().WithMessage(AiTarotErrorCode.InvalidCard);

        RuleFor(x => x.Locale)
            .NotEmpty()
            .WithMessage(AiTarotErrorCode.InvalidLocale)
            .Must(locale => locale is "en" or "vi")
            .WithMessage(AiTarotErrorCode.InvalidLocale);

        RuleFor(x => x.Cards)
            .NotNull()
            .NotEmpty()
            .WithMessage(AiTarotErrorCode.InvalidCardCount)
            .Must((request, cards) => cards.Count == (int)request.CardCount)
            .WithMessage(AiTarotErrorCode.InvalidCardCount);

        RuleForEach(x => x.Cards)
            .ChildRules(card =>
            {
                card.RuleFor(c => c.CardCode)
                    .NotEmpty()
                    .WithMessage(AiTarotErrorCode.InvalidCard)
                    .Must(TarotConstant.IsValidCardCode)
                    .WithMessage(AiTarotErrorCode.InvalidCard);
            });

        RuleFor(x => x.Cards)
            .Must(cards => cards.Select(c => c.CardCode).Distinct().Count() == cards.Count)
            .WithMessage(AiTarotErrorCode.InvalidCard);
    }
}
