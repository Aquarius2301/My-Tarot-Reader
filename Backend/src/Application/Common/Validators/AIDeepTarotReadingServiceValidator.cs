using FluentValidation;
using MyTarotReader.Application.Constants.Errors;
using MyTarotReader.Application.Constants.Tarot;
using MyTarotReader.Application.Contracts.Services;
using MyTarotReader.Domain.Enums;

namespace MyTarotReader.Application.Common.Validators;

/// <summary>
/// The rules every deep tarot create request shares: a supported locale and a non-null,
/// non-empty, duplicate-free list of valid cards. Each spread's validator derives from this
/// and only adds its own card count.
/// </summary>
/// <remarks>
/// Every rule below is null-safe so a malformed <c>cards: null</c> body is rejected with a 400
/// instead of throwing a NullReferenceException (500).
/// </remarks>
public abstract class CreateDeepTarotReadingRequestValidatorBase<TRequest>
    : AbstractValidator<TRequest>
    where TRequest : ICreateDeepTarotReadingRequest
{
    protected CreateDeepTarotReadingRequestValidatorBase()
    {
        RuleFor(x => x.Locale)
            .NotEmpty()
            .WithMessage(AiDeepTarotErrorCode.InvalidLocale)
            .Must(locale => locale is "en" or "vi")
            .WithMessage(AiDeepTarotErrorCode.InvalidLocale);

        RuleFor(x => x.Cards)
            .NotNull()
            .WithMessage(AiDeepTarotErrorCode.InvalidCardCount)
            .NotEmpty()
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

    /// <summary>
    /// Adds the rule requiring exactly the number of cards declared by the topic's spread.
    /// </summary>
    protected void RequireCardCount(DeepTarotTopic topic) =>
        RuleFor(x => x.Cards)
            .Must(cards =>
                cards is not null
                && cards.Count == DeepTarotConstant.GetRequiredCardCount(topic)
            )
            .WithMessage(AiDeepTarotErrorCode.InvalidCardCount);
}

/// <summary>
/// Validates the 12 astrological houses create request. The topic is fixed by the route, so the
/// client cannot ask for a different spread.
/// </summary>
public class CreateTwelveHousesReadingRequestValidator
    : CreateDeepTarotReadingRequestValidatorBase<CreateTwelveHousesReadingRequest>
{
    public CreateTwelveHousesReadingRequestValidator() => RequireCardCount(DeepTarotTopic.TwelveHouses);
}

/// <summary>
/// Validates the 12 months create request. The topic is fixed by the route, so the client
/// cannot ask for a different spread.
/// </summary>
public class CreateTwelveMonthsReadingRequestValidator
    : CreateDeepTarotReadingRequestValidatorBase<CreateTwelveMonthsReadingRequest>
{
    public CreateTwelveMonthsReadingRequestValidator() => RequireCardCount(DeepTarotTopic.TwelveMonths);
}

/// <summary>
/// Validates the crossroads create request. The topic is fixed by the route, so the client
/// cannot ask for a different spread; the option count is not, because it sizes the spread.
/// </summary>
public class CreateCrossroadsReadingRequestValidator
    : CreateDeepTarotReadingRequestValidatorBase<CreateCrossroadsReadingRequest>
{
    public CreateCrossroadsReadingRequestValidator()
    {
        RuleFor(x => x.Question)
            .NotEmpty()
            .WithMessage(AiDeepTarotErrorCode.InvalidQuestion)
            .MaximumLength(DeepTarotConstant.CrossroadsQuestionMaxLength)
            .WithMessage(AiDeepTarotErrorCode.InvalidQuestion);

        RuleFor(x => x.Options)
            .NotNull()
            .WithMessage(AiDeepTarotErrorCode.InvalidOption)
            .Must(options =>
                options is not null && DeepTarotConstant.IsValidCrossroadsOptionCount(options.Count)
            )
            .WithMessage(AiDeepTarotErrorCode.InvalidOption);

        RuleForEach(x => x.Options)
            .NotEmpty()
            .WithMessage(AiDeepTarotErrorCode.InvalidOption)
            .MaximumLength(DeepTarotConstant.CrossroadsOptionMaxLength)
            .WithMessage(AiDeepTarotErrorCode.InvalidOption);

        // Two options that differ only by casing or surrounding whitespace are the same choice.
        RuleFor(x => x.Options)
            .Must(options =>
                options is null
                || options
                    .Select(option => option?.Trim().ToLowerInvariant())
                    .Distinct()
                    .Count()
                    == options.Count
            )
            .WithMessage(AiDeepTarotErrorCode.InvalidOption);

        RuleFor(x => x.TimeFrame)
            .IsInEnum()
            .WithMessage(AiDeepTarotErrorCode.InvalidTimeFrame)
            .When(timeFrame => timeFrame is not null);

        // The spread is sized by the options, so the count cannot come from the topic alone.
        RuleFor(x => x.Cards)
            .Must((x, cards) =>
                cards is not null
                && cards.Count
                    == DeepTarotConstant.GetCrossroadsCardCount(x.Options?.Count ?? 0)
            )
            .WithMessage(AiDeepTarotErrorCode.InvalidCardCount);
    }
}
