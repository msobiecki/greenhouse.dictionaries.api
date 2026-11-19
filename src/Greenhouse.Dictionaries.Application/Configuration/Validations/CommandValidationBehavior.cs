using FluentValidation;
using Greenhouse.Dictionaries.Application.Configuration.Commands;
using MediatR;

namespace Greenhouse.Dictionaries.Application.Configuration.Validations;

public class CommandValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators) : IPipelineBehavior<TRequest, TResponse> where TRequest : class, ICommand<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (!validators.Any())
            return await next(cancellationToken);

        var failures = new List<FluentValidation.Results.ValidationFailure>();
        foreach (var validator in validators)
        {
            var result = await validator.ValidateAsync(
                new ValidationContext<TRequest>(request), cancellationToken);
            failures.AddRange(result.Errors);
        }

        var errors = failures
            .Where(failure => failure is not null)
            .GroupBy(failure => failure.PropertyName, failure => failure.ErrorMessage,
                (propertyName, messages) => new FluentValidation.Results.ValidationFailure
                {
                    PropertyName = propertyName,
                    ErrorMessage = string.Join(", ", messages.Distinct())
                })
            .ToList();

        if (errors.Count > 0)
            throw new ValidationException("Invalid command.", errors);

        return await next(cancellationToken);
    }
}
