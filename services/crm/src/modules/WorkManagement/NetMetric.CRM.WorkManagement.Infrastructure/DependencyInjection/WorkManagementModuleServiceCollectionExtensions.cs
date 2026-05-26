// <copyright file="WorkManagementModuleServiceCollectionExtensions.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using NetMetric.CRM.WorkManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.WorkManagement.Application.Commands.Tasks.CreateWorkTask;
using NetMetric.CRM.WorkManagement.Infrastructure.Health;
using NetMetric.CRM.WorkManagement.Infrastructure.Persistence;

namespace NetMetric.CRM.WorkManagement.Infrastructure.DependencyInjection;

public static class WorkManagementModuleServiceCollectionExtensions
{
    public static IServiceCollection AddWorkManagementModule(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("WorkManagementConnection")
            ?? configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("WorkManagementConnection connection string not found.");

        services.AddDbContext<WorkManagementDbContext>(options =>
            options.UseSqlServer(connectionString, sql => sql.MigrationsAssembly(typeof(WorkManagementDbContext).Assembly.FullName)));

        services.AddScoped<IWorkManagementDbContext>(sp => sp.GetRequiredService<WorkManagementDbContext>());

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<CreateWorkTaskCommand>());
        services.AddValidatorsFromAssemblyContaining<CreateWorkTaskCommandValidator>();

        services.AddHealthChecks()
            .AddCheck<WorkManagementDbContextHealthCheck>(
                name: "work-management-db",
                failureStatus: HealthStatus.Unhealthy,
                tags: ["ready", "work-management"]);

        return services;
    }
}
