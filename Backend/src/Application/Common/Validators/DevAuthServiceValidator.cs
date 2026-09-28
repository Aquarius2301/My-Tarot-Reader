using FluentValidation;
using MyTarotReader.Application.Constants.Errors;
using MyTarotReader.Application.Contracts.Services;

namespace MyTarotReader.Application.Common.Validators;

public class CreateDevTokenRequestValidator : AbstractValidator<CreateDevTokenRequest>
{
    public CreateDevTokenRequestValidator()
    {
        RuleFor(x => x.Role).IsInEnum().WithMessage(DevAuthErrorCode.InvalidRole);
    }
}
