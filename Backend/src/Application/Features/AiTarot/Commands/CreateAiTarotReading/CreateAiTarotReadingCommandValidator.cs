using System;
using FluentValidation;
using MyTarotReader.Application.Constants.Errors;

namespace MyTarotReader.Application.Features.AiTarot;

public class CreateAiTarotReadingCommandValidator : AbstractValidator<CreateAiTarotReadingCommand>
{
    public CreateAiTarotReadingCommandValidator()
    {
        RuleFor(x => x.Locale)
            .Must(locale => locale == "vi" || locale == "en" || locale == null)
            .WithMessage(AiTarotErrorCode.InvalidLocale);

        // RuleFor(x => x.Cards)
        //     .NotEmpty()
        // .WithMessage(AiTarotErrorCode.EmptyCards);
    }
}
