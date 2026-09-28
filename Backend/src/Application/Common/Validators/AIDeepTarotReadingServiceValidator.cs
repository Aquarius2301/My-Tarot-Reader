using FluentValidation;
using MyTarotReader.Application.Constants.Errors;
using MyTarotReader.Application.Constants.Tarot;
using MyTarotReader.Application.Contracts.Services;

namespace MyTarotReader.Application.Common.Validators;

public class CreateAiDeepTarotReadingRequestValidator
    : AbstractValidator<CreateAiDeepTarotReadingRequest>
{
    public CreateAiDeepTarotReadingRequestValidator()
    {
        RuleFor(x => x.Topic)
            .IsInEnum()
            .WithMessage(AiDeepTarotErrorCode.InvalidTopic)
            .Must(DeepTarotConstant.IsSupported)
            .WithMessage(AiDeepTarotErrorCode.TopicNotSupported);

        RuleFor(x => x.Locale)
            .NotEmpty()
            .WithMessage(AiDeepTarotErrorCode.InvalidLocale)
            .Must(locale => locale is "en" or "vi")
            .WithMessage(AiDeepTarotErrorCode.InvalidLocale);

        // Every card rule below is null-safe so a malformed `cards: null` body is rejected
        // with a 400 instead of throwing a NullReferenceException (500).
        RuleFor(x => x.Cards)
            .NotNull()
            .WithMessage(AiDeepTarotErrorCode.InvalidCardCount)
            .NotEmpty()
            .WithMessage(AiDeepTarotErrorCode.InvalidCardCount)
            .Must((request, cards) =>
                cards is not null
                && (
                    !DeepTarotConstant.IsSupported(request.Topic)
                    || cards.Count == DeepTarotConstant.GetRequiredCardCount(request.Topic)
                )
            )
            .WithMessage(AiDeepTarotErrorCode.InvalidCardCount)
            .Must(cards =>
                cards is not null
                && cards.Select(c => c.CardCode).Distinct().Count() == cards.Count
            )
            .WithMessage(AiDeepTarotErrorCode.InvalidCard);

        RuleForEach(x => x.Cards)
            .ChildRules(card =>
            {
                card.RuleFor(c => c.CardCode)
                    .NotEmpty()
                    .WithMessage(AiDeepTarotErrorCode.InvalidCard)
                    .Must(TarotConstant.IsValidCardCode)
                    .WithMessage(AiDeepTarotErrorCode.InvalidCard);
            });
    }
}
