// <copyright file="QuoteManagementModuleServiceCollectionExtensions.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using NetMetric.CRM.QuoteManagement.Application.Abstractions.Integration;
using NetMetric.CRM.QuoteManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.QuoteManagement.Application.Abstractions.Services;
using NetMetric.CRM.QuoteManagement.Application.Commands.Quotes;
using NetMetric.CRM.QuoteManagement.Application.Services;
using NetMetric.CRM.QuoteManagement.Application.Validators;
using NetMetric.CRM.QuoteManagement.Contracts.Integration;
using NetMetric.CRM.QuoteManagement.Infrastructure.Health;
using NetMetric.CRM.QuoteManagement.Infrastructure.Outbox;
using NetMetric.CRM.QuoteManagement.Infrastructure.Persistence;
using NetMetric.CRM.QuoteManagement.Infrastructure.Services;
using NetMetric.Messaging.RabbitMq.DependencyInjection;
using NetMetric.Persistence.EntityFrameworkCore.Auditing;
using NetMetric.Persistence.EntityFrameworkCore.SoftDelete;
using NetMetric.Persistence.EntityFrameworkCore.Tenancy;
using NetMetric.Repository;
using NetMetric.Repository.EntityFrameworkCore;

namespace NetMetric.CRM.QuoteManagement.Infrastructure.DependencyInjection;

public static class QuoteManagementModuleServiceCollectionExtensions
{
    public static IServiceCollection AddQuoteManagementModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<QuoteManagementDbContext>((sp, options) =>
        {
            var connectionString = configuration.GetConnectionString("QuoteManagementConnection")
                ?? throw new InvalidOperationException("QuoteManagementConnection connection string not found.");

            options.UseSqlServer(
                connectionString,
                sql => sql.MigrationsAssembly(typeof(QuoteManagementDbContext).Assembly.FullName));

            options.AddInterceptors(
                sp.GetRequiredService<TenantIsolationSaveChangesInterceptor>(),
                sp.GetRequiredService<AuditSaveChangesInterceptor>(),
                sp.GetRequiredService<SoftDeleteSaveChangesInterceptor>());
        });

        services.AddScoped<IQuoteManagementDbContext>(sp => sp.GetRequiredService<QuoteManagementDbContext>());
        services.AddScoped<IQuoteProductReadModelSyncService, QuoteProductReadModelSyncService>();
        services.AddScoped<IQuoteOpportunityReadModelSyncService, QuoteOpportunityReadModelSyncService>();
        services.AddScoped<IQuoteCustomerReadModelSyncService, QuoteCustomerReadModelSyncService>();
        services.AddScoped<IQuoteCrossWriterService, QuoteCrossWriterService>();
        services.AddScoped<IQuoteManagementOutbox, QuoteManagementOutbox>();
        services.AddRabbitMqMessaging(configuration);
        services.AddOptions<QuoteManagementOutboxProcessorOptions>()
            .Bind(configuration.GetSection(QuoteManagementOutboxProcessorOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();
        services.AddSingleton<QuoteManagementOutboxMetrics>();
        services.AddScoped<IQuoteManagementOutboxPublisher, RabbitMqQuoteManagementOutboxPublisher>();
        services.AddScoped<IQuoteManagementOutboxProcessor, QuoteManagementOutboxProcessor>();
        services.AddHostedService<QuoteManagementOutboxBackgroundService>();
        services.AddScoped(typeof(IRepository<,>), typeof(EfRepository<,>));
        services.AddValidatorsFromAssemblyContaining<CreateQuoteCommandValidator>();
        services.AddValidatorsFromAssemblyContaining<ProposalTemplateValidator>();
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<CreateQuoteCommand>());
        services.AddHealthChecks()
            .AddCheck<QuoteManagementDbContextHealthCheck>(
                name: "quote-management-db",
                failureStatus: HealthStatus.Unhealthy,
                tags: ["ready", "db", "quote-management"]);

        return services;
    }
}
