﻿using FluentValidation.Results;
using FluentValidation;
using MediatR;

namespace Application.Common.Behaviours.Validation;

public class RequestValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public RequestValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken,
        RequestHandlerDelegate<TResponse> next)
    {
        // ValidationContext<object> context = new(request);
        // List<ValidationFailure> failures = _validators
        //     .Select(validator => validator.Validate(context))
        //     .SelectMany(result => result.Errors)
        //     .Where(failure => failure != null)
        //     .ToList();
        // if (failures.Count != 0) throw new ValidationException(failures);
        var context = new ValidationContext<TRequest>(request);
        var validatonResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var failures = validatonResults
            .Where(r => r.Errors.Any())
            .SelectMany(r => r.Errors)
            .ToList();

        if (failures.Any())
            throw new ValidationException(failures);

        return await next();
    }
}