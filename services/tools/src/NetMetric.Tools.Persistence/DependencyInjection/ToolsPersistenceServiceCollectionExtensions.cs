// <copyright file="ToolsPersistenceServiceCollectionExtensions.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NetMetric.Tools.Application.Abstractions.Persistence;

namespace NetMetric.Tools.Persistence.DependencyInjection;

public static class ToolsPersistenceServiceCollectionExtensions
{
    public static IServiceCollection AddToolsPersistence(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<ToolsDbContext>(options =>
        {
            options.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.MigrationsAssembly(typeof(ToolsDbContext).Assembly.FullName);
                sqlOptions.EnableRetryOnFailure(3);
            });
        });

        services.AddScoped<IToolsDbContext>(provider => provider.GetRequiredService<ToolsDbContext>());
        return services;
    }
}
