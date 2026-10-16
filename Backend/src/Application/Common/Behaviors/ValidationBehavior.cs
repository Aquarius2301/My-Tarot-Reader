using FluentValidation;
using MediatR;
using MyTarotReader.Application.Common.Models;

namespace MyTarotReader.Application.Common.Behaviors;

public class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken
    )
    {
        if (!validators.Any())
        {
            return await next(cancellationToken);
        }

        var validationResults = await Task.WhenAll(
            validators.Select(validator => validator.ValidateAsync(request, cancellationToken))
        );

        var fieldErrors = validationResults
            .SelectMany(result => result.Errors)
            .Where(error => error is not null)
            .Select(error => new FieldError(error.PropertyName, error.ErrorMessage))
            .ToList();

        if (fieldErrors.Count > 0)
        {
            throw new Exceptions.BadRequestException(fieldErrors: fieldErrors);
        }

        return await next(cancellationToken);
    }
}
