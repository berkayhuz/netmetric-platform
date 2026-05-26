// <copyright file="SalesForecastingModuleServiceCollectionExtensions.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using NetMetric.CRM.SalesForecasting.Application.Abstractions.Persistence;
using NetMetric.CRM.SalesForecasting.Application.Handlers;
using NetMetric.CRM.SalesForecasting.Domain.Common;
using NetMetric.CRM.SalesForecasting.Infrastructure.Health;
using NetMetric.CRM.SalesForecasting.Infrastructure.Persistence;

namespace NetMetric.CRM.SalesForecasting.Infrastructure.DependencyInjection;

public static class SalesForecastingModuleServiceCollectionExtensions
{
    public static IServiceCollection AddSalesForecastingModule(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("SalesForecastingConnection")
            ?? configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("SalesForecastingConnection connection string not found.");

        services.AddDbContext<SalesForecastingDbContext>(options =>
        {
            options.UseSqlServer(connectionString, sql => sql.MigrationsAssembly(typeof(SalesForecastingDbContext).Assembly.FullName));
        });

        services.AddScoped<ISalesForecastingDbContext>(sp => sp.GetRequiredService<SalesForecastingDbContext>());
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(GetSalesForecastSummaryQueryHandler).Assembly));
        services.AddValidatorsFromAssembly(typeof(GetSalesForecastSummaryQueryHandler).Assembly);
        services.AddValidatorsFromAssembly(typeof(ISalesForecastingModuleMarker).Assembly);

        services.AddHealthChecks()
            .AddCheck<SalesForecastingDbContextHealthCheck>(
                name: "sales-forecasting-db",
                failureStatus: HealthStatus.Unhealthy,
                tags: ["ready", "db", "sales-forecasting"]);

        return services;
    }
}
