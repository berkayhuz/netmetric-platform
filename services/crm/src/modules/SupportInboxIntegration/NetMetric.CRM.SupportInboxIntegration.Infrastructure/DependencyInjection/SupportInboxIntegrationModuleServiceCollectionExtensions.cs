// <copyright file="SupportInboxIntegrationModuleServiceCollectionExtensions.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using NetMetric.CRM.SupportInboxIntegration.Application.Abstractions.Persistence;
using NetMetric.CRM.SupportInboxIntegration.Application.Abstractions.Services;
using NetMetric.CRM.SupportInboxIntegration.Application.Validators;
using NetMetric.CRM.SupportInboxIntegration.Infrastructure.Health;
using NetMetric.CRM.SupportInboxIntegration.Infrastructure.Persistence;
using NetMetric.CRM.SupportInboxIntegration.Infrastructure.Services;

namespace NetMetric.CRM.SupportInboxIntegration.Infrastructure.DependencyInjection;

public static class SupportInboxIntegrationModuleServiceCollectionExtensions
{
    public static IServiceCollection AddSupportInboxIntegrationModule(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("SupportInboxIntegrationConnection")
            ?? configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("SupportInboxIntegrationConnection connection string not found.");

        services.AddDbContext<SupportInboxIntegrationDbContext>((sp, options) =>
        {
            options.UseSqlServer(connectionString, sql => sql.MigrationsAssembly(typeof(SupportInboxIntegrationDbContext).Assembly.FullName));
        });

        services.AddScoped<ISupportInboxIntegrationDbContext>(sp => sp.GetRequiredService<SupportInboxIntegrationDbContext>());
        services.AddScoped<ISupportInboxSynchronizationService, SupportInboxSynchronizationService>();
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ISupportInboxIntegrationDbContext).Assembly));
        services.AddValidatorsFromAssemblyContaining<CreateSupportInboxConnectionCommandValidator>();
        services.AddHealthChecks().AddCheck<SupportInboxIntegrationDbContextHealthCheck>("support-inbox-integration-db", HealthStatus.Unhealthy, ["ready", "db", "support-inbox"]);

        return services;
    }
}
