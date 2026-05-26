// <copyright file="DealManagementModuleServiceCollectionExtensions.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using NetMetric.CRM.DealManagement.Application.Abstractions.Integration;
using NetMetric.CRM.DealManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.DealManagement.Application.Handlers;
using NetMetric.CRM.DealManagement.Domain.Common;
using NetMetric.CRM.DealManagement.Infrastructure.Health;
using NetMetric.CRM.DealManagement.Infrastructure.Outbox;
using NetMetric.CRM.DealManagement.Infrastructure.Persistence;
using NetMetric.CRM.DealManagement.Infrastructure.Services;
using NetMetric.Messaging.RabbitMq.DependencyInjection;

namespace NetMetric.CRM.DealManagement.Infrastructure.DependencyInjection;

public static class DealManagementModuleServiceCollectionExtensions
{
    public static IServiceCollection AddDealManagementModule(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DealManagementConnection")
            ?? configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("DealManagementConnection connection string not found.");

        services.AddDbContext<DealManagementDbContext>(options =>
        {
            options.UseSqlServer(connectionString, sql => sql.MigrationsAssembly(typeof(DealManagementDbContext).Assembly.FullName));
        });

        services.AddScoped<IDealManagementDbContext>(sp => sp.GetRequiredService<DealManagementDbContext>());
        services.AddScoped<IDealManagementOutbox, DealManagementOutbox>();
        services.AddRabbitMqMessaging(configuration);
        services.AddOptions<DealManagementOutboxProcessorOptions>()
            .Bind(configuration.GetSection(DealManagementOutboxProcessorOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();
        services.AddSingleton<DealManagementOutboxMetrics>();
        services.AddScoped<IDealManagementOutboxPublisher, RabbitMqDealManagementOutboxPublisher>();
        services.AddScoped<IDealManagementOutboxProcessor, DealManagementOutboxProcessor>();
        services.AddHostedService<DealManagementOutboxBackgroundService>();
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreateDealCommandHandler).Assembly));
        services.AddValidatorsFromAssembly(typeof(IDealManagementModuleMarker).Assembly);
        services.AddValidatorsFromAssembly(typeof(CreateDealCommandHandler).Assembly);
        services.AddHealthChecks().AddCheck<DealManagementDbContextHealthCheck>(
        name: "deal-management-db",
        failureStatus: HealthStatus.Unhealthy,
        tags: ["ready", "db", "deal-management"]);

        return services;
    }
}
