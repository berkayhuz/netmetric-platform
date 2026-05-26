// <copyright file="TenantManagementModuleServiceCollectionExtensions.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using NetMetric.CRM.TenantManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.TenantManagement.Application.Commands.ProvisionTenant;
using NetMetric.CRM.TenantManagement.Infrastructure.Health;
using NetMetric.CRM.TenantManagement.Infrastructure.Persistence;

namespace NetMetric.CRM.TenantManagement.Infrastructure.DependencyInjection;

public static class TenantManagementModuleServiceCollectionExtensions
{
    public static IServiceCollection AddTenantManagementModule(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("TenantManagementConnection")
            ?? configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("TenantManagementConnection connection string not found.");

        services.AddDbContext<TenantManagementDbContext>(options =>
            options.UseSqlServer(connectionString, sql => sql.MigrationsAssembly(typeof(TenantManagementDbContext).Assembly.FullName)));

        services.AddScoped<ITenantManagementDbContext>(sp => sp.GetRequiredService<TenantManagementDbContext>());
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ProvisionTenantCommandHandler).Assembly));
        services.AddValidatorsFromAssembly(typeof(ProvisionTenantCommandHandler).Assembly);

        services.AddHealthChecks()
            .AddCheck<TenantManagementDbContextHealthCheck>(
                "tenant-management-db",
                HealthStatus.Unhealthy,
                ["db", "tenant-management", "ready"]);

        return services;
    }
}
