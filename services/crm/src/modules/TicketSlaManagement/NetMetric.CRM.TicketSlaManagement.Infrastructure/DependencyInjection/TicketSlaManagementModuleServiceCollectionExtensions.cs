// <copyright file="TicketSlaManagementModuleServiceCollectionExtensions.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using NetMetric.CRM.TicketManagement.Infrastructure.Health;
using NetMetric.CRM.TicketSlaManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.TicketSlaManagement.Application.Abstractions.Services;
using NetMetric.CRM.TicketSlaManagement.Application.Validators;
using NetMetric.CRM.TicketSlaManagement.Infrastructure.Persistence;
using NetMetric.CRM.TicketSlaManagement.Infrastructure.Services;

namespace NetMetric.CRM.TicketSlaManagement.Infrastructure.DependencyInjection;

public static class TicketSlaManagementModuleServiceCollectionExtensions
{
    public static IServiceCollection AddTicketSlaManagementModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<TicketSlaManagementDbContext>((sp, options) =>
        {
            var connectionString = configuration.GetConnectionString("TicketSlaManagementConnection")
                ?? throw new InvalidOperationException("TicketSlaManagementConnection connection string not found.");

            options.UseSqlServer(
                connectionString,
                sql => sql.MigrationsAssembly(typeof(TicketSlaManagementDbContext).Assembly.FullName));
        });

        services.AddScoped<ITicketSlaManagementDbContext>(sp => sp.GetRequiredService<TicketSlaManagementDbContext>());
        services.AddScoped<ITicketSlaAdministrationService, TicketSlaAdministrationService>();

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ITicketSlaManagementDbContext).Assembly));
        services.AddValidatorsFromAssemblyContaining<CreateSlaPolicyCommandValidator>();

        services.AddHealthChecks()
            .AddCheck<TicketSlaManagementDbContextHealthCheck>(
                name: "ticket-sla-management-db",
                failureStatus: HealthStatus.Unhealthy,
                tags: ["ready", "db", "ticket-sla-management"]);

        return services;
    }
}
