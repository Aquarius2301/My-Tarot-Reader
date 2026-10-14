using FluentValidation;
using MyTarotReader.Application.Constants.Errors;
using MyTarotReader.Application.Constants.Tarot;
using MyTarotReader.Domain.Enums;

namespace MyTarotReader.Application.Features.AiDeepTarot;

public class CreateTwelveMonthsReadingCommandValidator
    : AbstractValidator<CreateTwelveMonthsReadingCommand>
{
    public CreateTwelveMonthsReadingCommandValidator()
    {
        RuleFor(x => x.Locale)
            .Must(locale => locale == "vi" || locale == "en" || locale == null)
            .WithMessage(AiDeepTarotErrorCode.InvalidLocale);

        RuleFor(x => x.Cards)
            .Must(cards =>
                cards is not null
                && cards.Count
                    == DeepTarotConstant.GetRequiredCardCount(DeepTarotTopic.TwelveMonths)
            )
            .WithMessage(AiDeepTarotErrorCode.InvalidCardCount);

        RuleFor(x => x.Cards)
            .Must(cards =>
                cards is not null && cards.All(card => TarotConstant.IsValidCardCode(card.CardCode))
            )
            .WithMessage(AiDeepTarotErrorCode.InvalidCard);
    }
}
