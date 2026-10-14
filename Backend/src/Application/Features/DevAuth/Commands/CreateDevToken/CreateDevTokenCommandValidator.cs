using FluentValidation;
using MyTarotReader.Application.Constants.Errors;

namespace MyTarotReader.Application.Features.DevAuth;

public class CreateDevTokenCommandValidator : AbstractValidator<CreateDevTokenCommand>
{
    public CreateDevTokenCommandValidator()
    {
        RuleFor(x => x.Role)
            .IsInEnum()
            .When(x => x.Role.HasValue)
            .WithMessage(DevAuthErrorCode.InvalidRole);
    }
}
