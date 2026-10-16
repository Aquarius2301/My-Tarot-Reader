using FluentValidation;
using Microsoft.EntityFrameworkCore;
using MyTarotReader.Application.Constants.Errors;
using MyTarotReader.Application.Constants.Tarot;
using MyTarotReader.Application.Persistence;

namespace MyTarotReader.Application.Features.AiTarot;

public class CreateAiTarotReadingCommandValidator : AbstractValidator<CreateAiTarotReadingCommand>
{
    public CreateAiTarotReadingCommandValidator(IAppDbContext context)
    {
        RuleFor(x => x.Locale)
            .Must(locale => locale == "vi" || locale == "en" || locale == null)
            .WithMessage(AiTarotErrorCode.InvalidLocale);

        RuleFor(x => x.CardCount)
            .MustAsync(
                async (cardCount, cancellationToken) =>
                    await context.AITarotReadingCounts.AnyAsync(
                        s => s.CardCount == cardCount && s.IsEnabled,
                        cancellationToken
                    )
            )
            .WithMessage(AiTarotErrorCode.InvalidCardCount);

        RuleFor(x => x.Type)
            .NotEmpty()
            .WithMessage(AiTarotErrorCode.InvalidQuestionType)
            .MustAsync(
                async (type, cancellationToken) =>
                    await context.AITarotReadingTypes.AnyAsync(
                        s => s.Code == type && s.IsEnabled,
                        cancellationToken
                    )
            )
            .WithMessage(AiTarotErrorCode.InvalidQuestionType);

        RuleFor(x => x.Cards)
            .NotEmpty()
            .WithMessage(AiTarotErrorCode.InvalidCard)
            .Must((command, cards) => cards.Count == command.CardCount)
            .WithMessage(AiTarotErrorCode.InvalidCardCount);

        RuleForEach(x => x.Cards)
            .Must(card => TarotConstant.IsValidCardCode(card.CardCode))
            .WithMessage(AiTarotErrorCode.InvalidCard);
    }
}
