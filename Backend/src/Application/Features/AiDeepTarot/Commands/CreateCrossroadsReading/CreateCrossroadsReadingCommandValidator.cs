using FluentValidation;
using MyTarotReader.Application.Constants.Errors;
using MyTarotReader.Application.Constants.Tarot;

namespace MyTarotReader.Application.Features.AiDeepTarot;

public class CreateCrossroadsReadingCommandValidator
    : AbstractValidator<CreateCrossroadsReadingCommand>
{
    public CreateCrossroadsReadingCommandValidator()
    {
        RuleFor(x => x.Locale)
            .Must(locale => locale == "vi" || locale == "en" || locale == null)
            .WithMessage(AiDeepTarotErrorCode.InvalidLocale);

        RuleFor(x => x.Question)
            .NotEmpty()
            .WithMessage(AiDeepTarotErrorCode.InvalidQuestion)
            .MaximumLength(DeepTarotConstant.CrossroadsQuestionMaxLength)
            .WithMessage(AiDeepTarotErrorCode.InvalidQuestion);

        RuleFor(x => x.Options)
            .Must(options =>
                options is not null
                && DeepTarotConstant.IsValidCrossroadsOptionCount(options.Count)
            )
            .WithMessage(AiDeepTarotErrorCode.InvalidOption);

        RuleFor(x => x.Options)
            .Must(options =>
                options is not null
                && options.All(option =>
                    !string.IsNullOrWhiteSpace(option)
                    && option.Length <= DeepTarotConstant.CrossroadsOptionMaxLength
                )
            )
            .WithMessage(AiDeepTarotErrorCode.InvalidOption);

        RuleFor(x => x.TimeFrame)
            .IsInEnum()
            .When(x => x.TimeFrame.HasValue)
            .WithMessage(AiDeepTarotErrorCode.InvalidTimeFrame);

        RuleFor(x => x.Cards)
            .Must((command, cards) =>
                cards is not null
                && cards.Count
                    == DeepTarotConstant.GetCrossroadsCardCount(command.Options?.Count ?? 0)
            )
            .WithMessage(AiDeepTarotErrorCode.InvalidCardCount);

        RuleFor(x => x.Cards)
            .Must(cards =>
                cards is not null && cards.All(card => TarotConstant.IsValidCardCode(card.CardCode))
            )
            .WithMessage(AiDeepTarotErrorCode.InvalidCard);
    }
}
