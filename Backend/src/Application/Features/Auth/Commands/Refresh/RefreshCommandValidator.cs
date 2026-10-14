using FluentValidation;
using MyTarotReader.Application.Constants.Errors;

namespace MyTarotReader.Application.Features.Auth;

public class RefreshValidator : AbstractValidator<RefreshCommand>
{
    public RefreshValidator()
    {
        RuleFor(x => x.RefreshToken).NotEmpty().WithMessage(AuthErrorCode.InvalidRefreshToken);

        RuleFor(x => x.DeviceFingerprint)
            .NotEmpty()
            .WithMessage(AuthErrorCode.InvalidDeviceFingerprint);
    }
}
