// <copyright file="AccountApplicationServiceCollectionExtensions.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using NetMetric.Account.Application.Abstractions.Security;
using NetMetric.Account.Application.Common;

namespace NetMetric.Account.Application.DependencyInjection;

public static class AccountApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddAccountApplication(this IServiceCollection services)
    {
        services.AddMediatR(configuration => configuration.RegisterServicesFromAssembly(typeof(IAccountApplicationMarker).Assembly));
        services.AddValidatorsFromAssembly(typeof(IAccountApplicationMarker).Assembly);
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddScoped<IReauthenticationService, ReauthenticationService>();

        return services;
    }
}
