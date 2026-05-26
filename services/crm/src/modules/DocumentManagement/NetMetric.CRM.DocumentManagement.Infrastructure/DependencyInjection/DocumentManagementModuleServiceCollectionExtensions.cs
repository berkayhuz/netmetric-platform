// <copyright file="DocumentManagementModuleServiceCollectionExtensions.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using NetMetric.CRM.DocumentManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.DocumentManagement.Application.Security;
using NetMetric.CRM.DocumentManagement.Infrastructure.Health;
using NetMetric.CRM.DocumentManagement.Infrastructure.Persistence;

namespace NetMetric.CRM.DocumentManagement.Infrastructure.DependencyInjection;

public static class DocumentManagementModuleServiceCollectionExtensions
{
    public static IServiceCollection AddDocumentManagementModule(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DocumentManagementConnection")
            ?? configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("DocumentManagementConnection connection string not found.");

        services.AddDbContext<DocumentManagementDbContext>((sp, options) =>
        {
            options.UseSqlServer(connectionString, sql => sql.MigrationsAssembly(typeof(DocumentManagementDbContext).Assembly.FullName));
        });

        services.AddScoped<IDocumentManagementDbContext>(sp => sp.GetRequiredService<DocumentManagementDbContext>());
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(IDocumentManagementDbContext).Assembly));
        services.AddValidatorsFromAssembly(typeof(IDocumentManagementDbContext).Assembly);

        services.AddHealthChecks()
            .AddCheck<DocumentManagementDbContextHealthCheck>(
                name: "documentmanagement-db",
                failureStatus: HealthStatus.Unhealthy,
                tags: ["ready", "db", "documentmanagement"]);

        return services;
    }
}
