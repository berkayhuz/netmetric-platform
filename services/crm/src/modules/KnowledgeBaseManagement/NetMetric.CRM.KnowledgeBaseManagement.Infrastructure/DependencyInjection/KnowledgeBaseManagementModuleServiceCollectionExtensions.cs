// <copyright file="KnowledgeBaseManagementModuleServiceCollectionExtensions.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using NetMetric.CRM.KnowledgeBaseManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.KnowledgeBaseManagement.Application.Validators;
using NetMetric.CRM.KnowledgeBaseManagement.Infrastructure.Health;
using NetMetric.CRM.KnowledgeBaseManagement.Infrastructure.Persistence;

namespace NetMetric.CRM.KnowledgeBaseManagement.Infrastructure.DependencyInjection;

public static class KnowledgeBaseManagementModuleServiceCollectionExtensions
{
    public static IServiceCollection AddKnowledgeBaseManagementModule(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("KnowledgeBaseManagementConnection")
            ?? configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("KnowledgeBaseManagementConnection connection string not found.");

        services.AddDbContext<KnowledgeBaseManagementDbContext>((sp, options) =>
        {
            options.UseSqlServer(connectionString, sql => sql.MigrationsAssembly(typeof(KnowledgeBaseManagementDbContext).Assembly.FullName));
        });

        services.AddScoped<IKnowledgeBaseManagementDbContext>(sp => sp.GetRequiredService<KnowledgeBaseManagementDbContext>());
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(IKnowledgeBaseManagementDbContext).Assembly));
        services.AddValidatorsFromAssemblyContaining<CreateKnowledgeBaseArticleCommandValidator>();
        services.AddHealthChecks().AddCheck<KnowledgeBaseManagementDbContextHealthCheck>("knowledge-base-management-db", HealthStatus.Unhealthy, ["ready", "db", "knowledge-base"]);

        return services;
    }
}
