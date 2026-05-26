// <copyright file="PipelineManagementModuleServiceCollectionExtensions.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NetMetric.Authorization.AspNetCore;
using NetMetric.CRM.PipelineManagement.Application;
using NetMetric.CRM.PipelineManagement.Application.Abstractions.Integration;
using NetMetric.CRM.PipelineManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.PipelineManagement.Application.Security;
using NetMetric.CRM.PipelineManagement.Application.Validators;
using NetMetric.CRM.PipelineManagement.Infrastructure.Outbox;
using NetMetric.CRM.PipelineManagement.Infrastructure.Persistence;
using NetMetric.Persistence.EntityFrameworkCore.Auditing;
using NetMetric.Persistence.EntityFrameworkCore.SoftDelete;
using NetMetric.Persistence.EntityFrameworkCore.Tenancy;

namespace NetMetric.CRM.PipelineManagement.Infrastructure.DependencyInjection;

public static class PipelineManagementModuleServiceCollectionExtensions
{
    public static IServiceCollection AddPipelineManagementModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<PipelineManagementDbContext>((sp, options) =>
        {
            var connectionString = configuration.GetConnectionString("PipelineManagementConnection")
                ?? throw new InvalidOperationException("PipelineManagementConnection connection string not found.");

            options.UseSqlServer(
                connectionString,
                sql =>
                {
                    sql.MigrationsAssembly(typeof(PipelineManagementDbContext).Assembly.FullName);
                    sql.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null);
                });

            options.AddInterceptors(
                sp.GetRequiredService<TenantIsolationSaveChangesInterceptor>(),
                sp.GetRequiredService<AuditSaveChangesInterceptor>(),
                sp.GetRequiredService<SoftDeleteSaveChangesInterceptor>());
        });

        services.AddScoped<IPipelineManagementDbContext>(sp => sp.GetRequiredService<PipelineManagementDbContext>());
        services.AddScoped<IPipelineManagementOutbox, Services.PipelineManagementOutbox>();
        services.AddOptions<PipelineManagementOutboxProcessorOptions>()
            .Bind(configuration.GetSection(PipelineManagementOutboxProcessorOptions.SectionName))
            .ValidateDataAnnotations();

        services.AddSingleton<PipelineManagementOutboxMetrics>();
        services.AddScoped<IPipelineManagementOutboxPublisher, RabbitMqPipelineManagementOutboxPublisher>();
        services.AddScoped<IPipelineManagementOutboxProcessor, PipelineManagementOutboxProcessor>();
        services.AddHostedService<PipelineManagementOutboxBackgroundService>();
        services.AddScoped<Application.Abstractions.Services.IPipelineValidationService, Application.Services.PipelineValidationService>();
        services.AddValidatorsFromAssemblyContaining<CreateLostReasonCommandValidator>();
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<PipelineManagementModule>());
        services.AddAuthorizationBuilder()
            .AddPolicy(PipelineManagementAuthorizationPolicies.LostReasonsManage,
                policy => policy.Requirements.Add(new PermissionRequirement(PipelineManagementPermissions.LostReasonsManage)))
            .AddPolicy(PipelineManagementAuthorizationPolicies.LostReasonsRead,
                policy => policy.Requirements.Add(new PermissionRequirement(PipelineManagementPermissions.LostReasonsRead)))
            .AddPolicy(PipelineManagementAuthorizationPolicies.OpportunityPipelineManage,
                policy => policy.Requirements.Add(new PermissionRequirement(PipelineManagementPermissions.OpportunityPipelineManage)))
            .AddPolicy(PipelineManagementAuthorizationPolicies.OpportunityStageHistoryRead,
                policy => policy.Requirements.Add(new PermissionRequirement(PipelineManagementPermissions.OpportunityStageHistoryRead)))
            .AddPolicy(PipelineManagementAuthorizationPolicies.LeadConversionsManage,
                policy => policy.Requirements.Add(new PermissionRequirement(PipelineManagementPermissions.LeadConversionsManage)))
            .AddPolicy(PipelineManagementAuthorizationPolicies.LeadConversionsRead,
                policy => policy.Requirements.Add(new PermissionRequirement(PipelineManagementPermissions.LeadConversionsRead)))
            .AddPolicy(PipelineManagementAuthorizationPolicies.PipelinesManage,
                policy => policy.Requirements.Add(new PermissionRequirement(PipelineManagementPermissions.PipelinesManage)))
            .AddPolicy(PipelineManagementAuthorizationPolicies.PipelinesRead,
                policy => policy.Requirements.Add(new PermissionRequirement(PipelineManagementPermissions.PipelinesRead)));

        services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();

        return services;
    }
}
