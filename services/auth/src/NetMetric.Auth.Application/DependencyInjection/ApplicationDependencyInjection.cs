// <copyright file="ApplicationDependencyInjection.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using NetMetric.Auth.Application.Behaviors;

namespace NetMetric.Auth.Application.DependencyInjection;

public static class ApplicationDependencyInjection
{
    public static IServiceCollection AddAuthApplication(this IServiceCollection services)
    {
        services.AddMediatR(configuration => configuration.RegisterServicesFromAssembly(typeof(ApplicationDependencyInjection).Assembly));
        services.AddValidatorsFromAssembly(typeof(ApplicationDependencyInjection).Assembly);
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        return services;
    }
}
