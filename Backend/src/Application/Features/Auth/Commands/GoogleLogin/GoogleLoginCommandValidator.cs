using FluentValidation;
using MyTarotReader.Application.Constants.Errors;

namespace MyTarotReader.Application.Features.Auth;

public class GoogleLoginCommandValidator : AbstractValidator<GoogleLoginCommand>
{
    public GoogleLoginCommandValidator()
    {
        RuleFor(x => x.Credential).NotEmpty().WithMessage(AuthErrorCode.InvalidKeyCredential);

        RuleFor(x => x.DeviceFingerprint)
            .NotEmpty()
            .WithMessage(AuthErrorCode.InvalidDeviceFingerprint);

        RuleFor(x => x.Locale)
            .Must(locale => locale == "vi" || locale == "en" || locale == null)
            .WithMessage(AuthErrorCode.InvalidLocale);
    }
}
