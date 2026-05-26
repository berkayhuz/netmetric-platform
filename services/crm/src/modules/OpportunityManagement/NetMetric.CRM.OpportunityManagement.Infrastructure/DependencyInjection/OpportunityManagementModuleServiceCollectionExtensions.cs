// <copyright file="OpportunityManagementModuleServiceCollectionExtensions.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using NetMetric.CRM.OpportunityManagement.Application.Abstractions.Integration;
using NetMetric.CRM.OpportunityManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.OpportunityManagement.Application.Features.Quotes.Commands.CreateQuote;
using NetMetric.CRM.OpportunityManagement.Contracts.Integration;
using NetMetric.CRM.OpportunityManagement.Application.Validators;
using NetMetric.CRM.OpportunityManagement.Infrastructure.Health;
using NetMetric.CRM.OpportunityManagement.Infrastructure.Outbox;
using NetMetric.CRM.OpportunityManagement.Infrastructure.Persistence;
using NetMetric.CRM.OpportunityManagement.Infrastructure.Services;
using NetMetric.Messaging.RabbitMq.DependencyInjection;
using NetMetric.Persistence.EntityFrameworkCore.Auditing;
using NetMetric.Persistence.EntityFrameworkCore.SoftDelete;
using NetMetric.Persistence.EntityFrameworkCore.Tenancy;
using NetMetric.Repository;
using NetMetric.Repository.EntityFrameworkCore;

namespace NetMetric.CRM.OpportunityManagement.Infrastructure.DependencyInjection;

public static class OpportunityManagementModuleServiceCollectionExtensions
{
    public static IServiceCollection AddOpportunityManagementModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<OpportunityManagementDbContext>((sp, options) =>
        {
            var connectionString = configuration.GetConnectionString("OpportunityManagementConnection")
                ?? throw new InvalidOperationException("OpportunityManagementConnection connection string not found.");

            options.UseSqlServer(connectionString, sql => sql.MigrationsAssembly(typeof(OpportunityManagementDbContext).Assembly.FullName));
            options.AddInterceptors(
                sp.GetRequiredService<TenantIsolationSaveChangesInterceptor>(),
                sp.GetRequiredService<AuditSaveChangesInterceptor>(),
                sp.GetRequiredService<SoftDeleteSaveChangesInterceptor>());
        });

        services.AddScoped<IOpportunityManagementDbContext>(sp => sp.GetRequiredService<OpportunityManagementDbContext>());
        services.AddScoped<IOpportunityCrossWriterService, OpportunityCrossWriterService>();
        services.AddScoped<IOpportunityManagementOutbox, OpportunityManagementOutbox>();
        services.AddRabbitMqMessaging(configuration);
        services.AddOptions<OpportunityManagementOutboxProcessorOptions>()
            .Bind(configuration.GetSection(OpportunityManagementOutboxProcessorOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();
        services.AddSingleton<OpportunityManagementOutboxMetrics>();
        services.AddScoped<IOpportunityManagementOutboxPublisher, RabbitMqOpportunityManagementOutboxPublisher>();
        services.AddScoped<IOpportunityManagementOutboxProcessor, OpportunityManagementOutboxProcessor>();
        services.AddHostedService<OpportunityManagementOutboxBackgroundService>();
        services.AddScoped(typeof(IRepository<,>), typeof(EfRepository<,>));
        services.AddValidatorsFromAssemblyContaining<CreateQuoteCommandValidator>();
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<CreateQuoteCommand>());
        services.AddHealthChecks()
            .AddCheck<OpportunityManagementDbContextHealthCheck>(
                name: "opportunity-management-db",
                failureStatus: HealthStatus.Unhealthy,
                tags: ["ready", "opportunity-management"]);

        return services;
    }
}
