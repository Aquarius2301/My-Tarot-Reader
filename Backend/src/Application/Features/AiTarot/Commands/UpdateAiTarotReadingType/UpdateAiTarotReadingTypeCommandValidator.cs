using FluentValidation;
using MyTarotReader.Application.Constants.Errors;

namespace MyTarotReader.Application.Features.AiTarot;

public class UpdateAiTarotReadingTypeCommandValidator
    : AbstractValidator<UpdateAiTarotReadingTypeCommand>
{
    public UpdateAiTarotReadingTypeCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage(AiTarotErrorCode.ReadingTypeNotFound);

        RuleFor(x => x.Code)
            .NotEmpty()
            .WithMessage(AiTarotErrorCode.InvalidReadingType)
            .MaximumLength(20)
            .WithMessage(AiTarotErrorCode.InvalidReadingType);

        RuleFor(x => x.NameEn)
            .NotEmpty()
            .WithMessage(AiTarotErrorCode.InvalidReadingType)
            .MaximumLength(100)
            .WithMessage(AiTarotErrorCode.InvalidReadingType);

        RuleFor(x => x.NameVi)
            .NotEmpty()
            .WithMessage(AiTarotErrorCode.InvalidReadingType)
            .MaximumLength(100)
            .WithMessage(AiTarotErrorCode.InvalidReadingType);
    }
}
