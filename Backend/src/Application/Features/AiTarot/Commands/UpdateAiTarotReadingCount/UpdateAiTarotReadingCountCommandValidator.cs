using FluentValidation;
using MyTarotReader.Application.Constants.Errors;

namespace MyTarotReader.Application.Features.AiTarot;

public class UpdateAiTarotReadingCountCommandValidator
    : AbstractValidator<UpdateAiTarotReadingCountCommand>
{
    public UpdateAiTarotReadingCountCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage(AiTarotErrorCode.ReadingCountNotFound);

        RuleFor(x => x.CardCount)
            .GreaterThan(0)
            .WithMessage(AiTarotErrorCode.InvalidReadingCount);

        RuleFor(x => x.WhiteCoins)
            .GreaterThanOrEqualTo(0)
            .WithMessage(AiTarotErrorCode.InvalidReadingCount);

        RuleFor(x => x.PositionsEn)
            .NotEmpty()
            .WithMessage(AiTarotErrorCode.InvalidReadingCount)
            .Must((command, positions) => positions != null && positions.Count == command.CardCount)
            .WithMessage(AiTarotErrorCode.InvalidReadingCount);

        RuleFor(x => x.PositionsVi)
            .NotEmpty()
            .WithMessage(AiTarotErrorCode.InvalidReadingCount)
            .Must((command, positions) => positions != null && positions.Count == command.CardCount)
            .WithMessage(AiTarotErrorCode.InvalidReadingCount);

        RuleFor(x => x.MinWords)
            .GreaterThan(0)
            .WithMessage(AiTarotErrorCode.InvalidReadingCount);

        RuleFor(x => x.MaxWords)
            .GreaterThanOrEqualTo(x => x.MinWords)
            .WithMessage(AiTarotErrorCode.InvalidReadingCount);
    }
}
