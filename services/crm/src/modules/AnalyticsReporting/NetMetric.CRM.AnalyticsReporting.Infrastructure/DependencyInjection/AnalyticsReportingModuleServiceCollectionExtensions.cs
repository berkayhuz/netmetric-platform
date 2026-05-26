// <copyright file="AnalyticsReportingModuleServiceCollectionExtensions.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using NetMetric.CRM.AnalyticsReporting.Application.Abstractions;
using NetMetric.CRM.AnalyticsReporting.Application.Abstractions.Persistence;
using NetMetric.CRM.AnalyticsReporting.Application.Queries.GetRoleDashboard;
using NetMetric.CRM.AnalyticsReporting.Infrastructure.Health;
using NetMetric.CRM.AnalyticsReporting.Infrastructure.Persistence;
using NetMetric.CRM.AnalyticsReporting.Infrastructure.Projection;

namespace NetMetric.CRM.AnalyticsReporting.Infrastructure.DependencyInjection;

public static class AnalyticsReportingModuleServiceCollectionExtensions
{
    public static IServiceCollection AddAnalyticsReportingModule(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("AnalyticsReportingConnection")
            ?? configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("AnalyticsReportingConnection connection string not found.");

        services.AddDbContext<AnalyticsReportingDbContext>(options =>
            options.UseSqlServer(connectionString, sql => sql.MigrationsAssembly(typeof(AnalyticsReportingDbContext).Assembly.FullName)));

        services.AddScoped<IAnalyticsReportingDbContext>(sp => sp.GetRequiredService<AnalyticsReportingDbContext>());
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(GetRoleDashboardQueryHandler).Assembly));
        services.AddValidatorsFromAssembly(typeof(GetRoleDashboardQueryHandler).Assembly);

        // Use the Action<TOptions> overload to bind the configuration section to AnalyticsProjectionOptions.
        // This avoids depending on the Configure(IServiceCollection, IConfigurationSection) extension being available.
        services.Configure<AnalyticsProjectionOptions>(options =>
            configuration.GetSection(AnalyticsProjectionOptions.SectionName).Bind(options));

        services.AddScoped<IAnalyticsProjectionSource, SqlServerAnalyticsProjectionSource>();
        services.AddScoped<IAnalyticsProjectionService, AnalyticsProjectionService>();
        services.AddHostedService<AnalyticsProjectionBackgroundService>();

        services.AddHealthChecks()
            .AddCheck<AnalyticsReportingDbContextHealthCheck>(
                "analytics-reporting-db",
                HealthStatus.Unhealthy,
                ["db", "analytics-reporting", "ready"])
            .AddCheck<AnalyticsProjectionWorkerHealthCheck>(
                "analytics-projection-worker",
                HealthStatus.Unhealthy,
                ["worker", "analytics-reporting", "ready"]);

        return services;
    }
}
