using FluentValidation;
using MyTarotReader.Application.Constants.Errors;

namespace MyTarotReader.Application.Features.Auth;

public class RefreshCommandValidator : AbstractValidator<RefreshCommand>
{
    public RefreshCommandValidator()
    {
        RuleFor(x => x.RefreshToken).NotEmpty().WithMessage(AuthErrorCode.InvalidRefreshToken);

        RuleFor(x => x.DeviceFingerprint)
            .NotEmpty()
            .WithMessage(AuthErrorCode.InvalidDeviceFingerprint);
    }
}
