// <copyright file="TagManagementModuleServiceCollectionExtensions.cs" company="NetMetric">
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
using NetMetric.CRM.TagManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.TagManagement.Application.Security;
using NetMetric.CRM.TagManagement.Infrastructure.Health;
using NetMetric.CRM.TagManagement.Infrastructure.Persistence;

namespace NetMetric.CRM.TagManagement.Infrastructure.DependencyInjection;

public static class TagManagementModuleServiceCollectionExtensions
{
    public static IServiceCollection AddTagManagementModule(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("TagManagementConnection")
            ?? configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("TagManagementConnection connection string not found.");

        services.AddDbContext<TagManagementDbContext>((sp, options) =>
        {
            options.UseSqlServer(connectionString, sql => sql.MigrationsAssembly(typeof(TagManagementDbContext).Assembly.FullName));
        });

        services.AddScoped<ITagManagementDbContext>(sp => sp.GetRequiredService<TagManagementDbContext>());
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ITagManagementDbContext).Assembly));
        services.AddValidatorsFromAssembly(typeof(ITagManagementDbContext).Assembly);
        services.AddAuthorizationBuilder()
            .AddPolicy(TagManagementPermissions.TagsRead,
                policy => policy.Requirements.Add(new PermissionRequirement(TagManagementPermissions.TagsRead)))
            .AddPolicy(TagManagementPermissions.TagsManage,
                policy => policy.Requirements.Add(new PermissionRequirement(TagManagementPermissions.TagsManage)))
            .AddPolicy(TagManagementPermissions.TagGroupsManage,
                policy => policy.Requirements.Add(new PermissionRequirement(TagManagementPermissions.TagGroupsManage)))
            .AddPolicy(TagManagementPermissions.SmartLabelsManage,
                policy => policy.Requirements.Add(new PermissionRequirement(TagManagementPermissions.SmartLabelsManage)))
            .AddPolicy(TagManagementPermissions.ClassificationsManage,
                policy => policy.Requirements.Add(new PermissionRequirement(TagManagementPermissions.ClassificationsManage)));
        services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();

        services.AddHealthChecks()
            .AddCheck<TagManagementDbContextHealthCheck>(
                name: "tagmanagement-db",
                failureStatus: HealthStatus.Unhealthy,
                tags: ["ready", "db", "tagmanagement"]);

        return services;
    }
}
