// <copyright file="ValidationBehavior.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentValidation;
using MediatR;

namespace NetMetric.Auth.Application.Behaviors;

public sealed class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!validators.Any())
        {
            return await next(cancellationToken);
        }

        var context = new ValidationContext<TRequest>(request);
        var failures = await Task.WhenAll(validators.Select(validator => validator.ValidateAsync(context, cancellationToken)));
        var errors = failures
            .SelectMany(result => result.Errors)
            .Where(error => error is not null)
            .ToList();

        if (errors.Count != 0)
        {
            throw new ValidationException(errors);
        }

        return await next(cancellationToken);
    }
}
