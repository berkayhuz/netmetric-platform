// <copyright file="TicketManagementModuleServiceCollectionExtensions.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using NetMetric.CRM.TicketManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.TicketManagement.Application.Abstractions.Services;
using NetMetric.CRM.TicketManagement.Application.Abstractions.Integration;
using NetMetric.CRM.TicketManagement.Application.Validators;
using NetMetric.CRM.TicketManagement.Infrastructure.Health;
using NetMetric.CRM.TicketManagement.Infrastructure.Outbox;
using NetMetric.CRM.TicketManagement.Infrastructure.Persistence;
using NetMetric.CRM.TicketManagement.Infrastructure.Services;
using NetMetric.MediatR;
using NetMetric.Messaging.RabbitMq.DependencyInjection;
using NetMetric.Persistence.EntityFrameworkCore.Auditing;
using NetMetric.Persistence.EntityFrameworkCore.SoftDelete;
using NetMetric.Persistence.EntityFrameworkCore.Tenancy;

namespace NetMetric.CRM.TicketManagement.Infrastructure.DependencyInjection;

public static class TicketManagementModuleServiceCollectionExtensions
{
    public static IServiceCollection AddTicketManagementModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.TryAddScoped<TenantIsolationSaveChangesInterceptor>();
        services.TryAddScoped<AuditSaveChangesInterceptor>();
        services.TryAddScoped<SoftDeleteSaveChangesInterceptor>();

        services.AddDbContext<TicketManagementDbContext>((sp, options) =>
        {
            var connectionString = configuration.GetConnectionString("TicketManagementConnection")
                ?? throw new InvalidOperationException("TicketManagementConnection connection string not found.");

            options.UseSqlServer(connectionString, sql =>
            {
                sql.MigrationsAssembly(typeof(TicketManagementDbContext).Assembly.FullName);
            });
        });

        services.AddScoped<ITicketManagementDbContext>(sp => sp.GetRequiredService<TicketManagementDbContext>());
        services.AddScoped<ITicketManagementOutbox, TicketManagementOutbox>();
        services.AddRabbitMqMessaging(configuration);
        services.AddOptions<TicketManagementOutboxProcessorOptions>()
            .Bind(configuration.GetSection(TicketManagementOutboxProcessorOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();
        services.AddSingleton<TicketManagementOutboxMetrics>();
        services.AddScoped<ITicketManagementOutboxPublisher, RabbitMqTicketManagementOutboxPublisher>();
        services.AddScoped<ITicketManagementOutboxProcessor, TicketManagementOutboxProcessor>();
        services.AddHostedService<TicketManagementOutboxBackgroundService>();
        services.AddScoped<ITicketAdministrationService, TicketAdministrationService>();

        services.AddValidatorsFromAssemblyContaining<CreateTicketCommandValidator>();

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(ITicketManagementDbContext).Assembly);
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        services.AddHealthChecks()
            .AddCheck<TicketManagementDbContextHealthCheck>(
                name: "ticket-management-db",
                failureStatus: HealthStatus.Unhealthy,
                tags: ["ready", "db", "ticket-management"]);

        return services;
    }
}
