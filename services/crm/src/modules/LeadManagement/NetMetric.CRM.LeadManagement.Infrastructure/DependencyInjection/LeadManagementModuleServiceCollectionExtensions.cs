// <copyright file="LeadManagementModuleServiceCollectionExtensions.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using NetMetric.CRM.LeadManagement.Application.Abstractions.Integration;
using NetMetric.CRM.LeadManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.LeadManagement.Application.Abstractions.Services;
using NetMetric.CRM.LeadManagement.Application.Commands.Leads;
using NetMetric.CRM.LeadManagement.Application.Validators;
using NetMetric.CRM.LeadManagement.Infrastructure.Health;
using NetMetric.CRM.LeadManagement.Infrastructure.Outbox;
using NetMetric.CRM.LeadManagement.Infrastructure.Persistence;
using NetMetric.CRM.LeadManagement.Infrastructure.Services;
using NetMetric.Messaging.RabbitMq.DependencyInjection;
using NetMetric.Persistence.EntityFrameworkCore.Auditing;
using NetMetric.Persistence.EntityFrameworkCore.SoftDelete;
using NetMetric.Persistence.EntityFrameworkCore.Tenancy;
using NetMetric.Repository;
using NetMetric.Repository.EntityFrameworkCore;

namespace NetMetric.CRM.LeadManagement.Infrastructure.DependencyInjection;

public static class LeadManagementModuleServiceCollectionExtensions
{
    public static IServiceCollection AddLeadManagementModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.TryAddScoped<TenantIsolationSaveChangesInterceptor>();
        services.TryAddScoped<AuditSaveChangesInterceptor>();
        services.TryAddScoped<SoftDeleteSaveChangesInterceptor>();

        services.AddDbContext<LeadManagementDbContext>((sp, options) =>
        {
            var connectionString = configuration.GetConnectionString("LeadManagementConnection")
                ?? throw new InvalidOperationException("LeadManagementConnection connection string not found.");

            options.UseSqlServer(
                connectionString,
                sql => sql.MigrationsAssembly(typeof(LeadManagementDbContext).Assembly.FullName));

            options.AddInterceptors(
                sp.GetRequiredService<TenantIsolationSaveChangesInterceptor>(),
                sp.GetRequiredService<AuditSaveChangesInterceptor>(),
                sp.GetRequiredService<SoftDeleteSaveChangesInterceptor>());
        });

        services.AddScoped<ILeadManagementDbContext>(sp => sp.GetRequiredService<LeadManagementDbContext>());
        services.AddScoped(typeof(IRepository<,>), typeof(EfRepository<,>));
        services.AddScoped<ILeadManagementOutbox, LeadManagementOutbox>();
        services.AddRabbitMqMessaging(configuration);
        services.AddOptions<LeadManagementOutboxProcessorOptions>()
            .Bind(configuration.GetSection(LeadManagementOutboxProcessorOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();
        services.AddSingleton<LeadManagementOutboxMetrics>();
        services.AddScoped<ILeadManagementOutboxPublisher, RabbitMqLeadManagementOutboxPublisher>();
        services.AddScoped<ILeadManagementOutboxProcessor, LeadManagementOutboxProcessor>();
        services.AddHostedService<LeadManagementOutboxBackgroundService>();
        services.AddScoped<ILeadAdministrationService, LeadAdministrationService>();
        services.AddScoped<ILeadCaptureService, LeadCaptureService>();
        services.AddScoped<ILeadRoutingService, LeadRoutingService>();
        services.AddScoped<ILeadScoringEngineService, LeadScoringEngineService>();

        services.AddValidatorsFromAssemblyContaining<CreateLeadCommandValidator>();
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<CreateLeadCommand>());

        services.AddHealthChecks()
            .AddCheck<LeadManagementDbContextHealthCheck>(
                name: "lead-management-db",
                failureStatus: HealthStatus.Unhealthy,
                tags: ["ready", "lead-management"]);

        return services;
    }
}
