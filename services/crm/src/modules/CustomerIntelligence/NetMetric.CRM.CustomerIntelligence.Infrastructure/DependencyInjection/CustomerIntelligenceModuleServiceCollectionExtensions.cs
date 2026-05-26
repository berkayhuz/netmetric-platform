// <copyright file="CustomerIntelligenceModuleServiceCollectionExtensions.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using NetMetric.Authorization.AspNetCore;
using NetMetric.CRM.CustomerIntelligence.Application.Abstractions.Insights;
using NetMetric.CRM.CustomerIntelligence.Application.Abstractions.Persistence;
using NetMetric.CRM.CustomerIntelligence.Application.Security;
using NetMetric.CRM.CustomerIntelligence.Infrastructure.Health;
using NetMetric.CRM.CustomerIntelligence.Infrastructure.Persistence;
using NetMetric.CRM.CustomerIntelligence.Infrastructure.Services;

namespace NetMetric.CRM.CustomerIntelligence.Infrastructure.DependencyInjection;

public static class CustomerIntelligenceModuleServiceCollectionExtensions
{
    public static IServiceCollection AddCustomerIntelligenceModule(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("CustomerIntelligenceConnection")
            ?? configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("CustomerIntelligenceConnection connection string not found.");

        services.AddDbContext<CustomerIntelligenceDbContext>((sp, options) =>
        {
            options.UseSqlServer(connectionString, sql => sql.MigrationsAssembly(typeof(CustomerIntelligenceDbContext).Assembly.FullName));
        });

        services.AddScoped<ICustomerIntelligenceDbContext>(sp => sp.GetRequiredService<CustomerIntelligenceDbContext>());
        services.AddScoped<ICustomerPortalSummaryMetricsProvider, CustomerPortalSummaryMetricsProvider>();
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ICustomerIntelligenceDbContext).Assembly));
        services.AddValidatorsFromAssembly(typeof(ICustomerIntelligenceDbContext).Assembly);
        services.AddAuthorization(options =>
        {
            options.AddPolicy(CustomerIntelligencePermissions.DuplicatesRead, policy => policy.Requirements.Add(new PermissionRequirement(CustomerIntelligencePermissions.DuplicatesRead)));
            options.AddPolicy(CustomerIntelligencePermissions.DuplicatesManage, policy => policy.Requirements.Add(new PermissionRequirement(CustomerIntelligencePermissions.DuplicatesManage)));
            options.AddPolicy(CustomerIntelligencePermissions.TimelineRead, policy => policy.Requirements.Add(new PermissionRequirement(CustomerIntelligencePermissions.TimelineRead)));
            options.AddPolicy(CustomerIntelligencePermissions.SearchRead, policy => policy.Requirements.Add(new PermissionRequirement(CustomerIntelligencePermissions.SearchRead)));
            options.AddPolicy(CustomerIntelligencePermissions.ViewsManage, policy => policy.Requirements.Add(new PermissionRequirement(CustomerIntelligencePermissions.ViewsManage)));
            options.AddPolicy(CustomerIntelligencePermissions.HealthRead, policy => policy.Requirements.Add(new PermissionRequirement(CustomerIntelligencePermissions.HealthRead)));
            options.AddPolicy(CustomerIntelligencePermissions.RelationsManage, policy => policy.Requirements.Add(new PermissionRequirement(CustomerIntelligencePermissions.RelationsManage)));
        });

        services.AddHealthChecks()
            .AddCheck<CustomerIntelligenceDbContextHealthCheck>(
                name: "customerintelligence-db",
                failureStatus: HealthStatus.Unhealthy,
                tags: ["ready", "db", "customerintelligence"]);

        return services;
    }
}
