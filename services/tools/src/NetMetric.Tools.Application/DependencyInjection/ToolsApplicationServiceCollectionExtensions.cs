// <copyright file="ToolsApplicationServiceCollectionExtensions.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.Extensions.DependencyInjection;

namespace NetMetric.Tools.Application.DependencyInjection;

public static class ToolsApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddToolsApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ToolsApplicationServiceCollectionExtensions).Assembly));
        return services;
    }
}
