using FluentValidation;
using MyTarotReader.Application.Common.Exceptions;
using MyTarotReader.Application.Common.Models;
using AppValidationException = MyTarotReader.Application.Common.Exceptions.ValidationException;

namespace MyTarotReader.Application.Common.Validators;

/// <summary>
/// Shared request-validation helper used across application services.
/// </summary>
public static class ValidationHelper
{
    /// <summary>
    /// Validates <paramref name="request"/> with <paramref name="validator"/> and throws a
    /// generic <see cref="BadRequestException"/> whose message is the first error's message
    /// (an i18n error code) when validation fails. The error lands in the response's
    /// <c>Message</c>, not field-level <c>Data</c>.
    /// </summary>
    public static void ValidateOrThrow<TRequest>(IValidator<TRequest> validator, TRequest request)
    {
        var result = validator.Validate(request);
        if (!result.IsValid)
        {
            throw new BadRequestException(result.Errors[0].ErrorMessage);
        }
    }

    /// <summary>
    /// Validates <paramref name="request"/> with <paramref name="validator"/> and throws a
    /// <see cref="AppValidationException"/> carrying every field error when validation fails.
    /// Field errors land in the response's <c>Data</c> (form-style errors), not just
    /// <c>Message</c>.
    /// </summary>
    public static void ValidateOrThrowForm<TRequest>(
        IValidator<TRequest> validator,
        TRequest request
    )
    {
        var result = validator.Validate(request);
        if (!result.IsValid)
        {
            throw new AppValidationException( // FluentValidator also has a ValidationException, so we alias it to AppValidationException
            [.. result.Errors.Select(e => new FieldError(e.PropertyName, e.ErrorMessage))]);
        }
    }
}
