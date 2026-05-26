// <copyright file="TicketWorkflowManagementModuleServiceCollectionExtensions.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using NetMetric.CRM.TicketWorkflowManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.TicketWorkflowManagement.Application.Validators;
using NetMetric.CRM.TicketWorkflowManagement.Infrastructure.Health;
using NetMetric.CRM.TicketWorkflowManagement.Infrastructure.Persistence;

namespace NetMetric.CRM.TicketWorkflowManagement.Infrastructure.DependencyInjection;

public static class TicketWorkflowManagementModuleServiceCollectionExtensions
{
    public static IServiceCollection AddTicketWorkflowManagementModule(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("TicketWorkflowManagementConnection")
            ?? configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("TicketWorkflowManagementConnection connection string not found.");

        services.AddDbContext<TicketWorkflowManagementDbContext>((sp, options) =>
        {
            options.UseSqlServer(connectionString, sql => sql.MigrationsAssembly(typeof(TicketWorkflowManagementDbContext).Assembly.FullName));
        });

        services.AddScoped<ITicketWorkflowManagementDbContext>(sp => sp.GetRequiredService<TicketWorkflowManagementDbContext>());
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ITicketWorkflowManagementDbContext).Assembly));
        services.AddValidatorsFromAssemblyContaining<CreateTicketQueueCommandValidator>();

        services.AddHealthChecks()
            .AddCheck<TicketWorkflowManagementDbContextHealthCheck>(
                name: "ticket-workflow-management-db",
                failureStatus: HealthStatus.Unhealthy,
                tags: ["ready", "db", "ticket-workflow-management"]);

        return services;
    }
}
