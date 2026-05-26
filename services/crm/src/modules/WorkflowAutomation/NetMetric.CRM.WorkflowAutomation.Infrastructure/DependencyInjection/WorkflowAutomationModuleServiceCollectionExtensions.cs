// <copyright file="WorkflowAutomationModuleServiceCollectionExtensions.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using NetMetric.CRM.WorkflowAutomation.Application.Abstractions.Persistence;
using NetMetric.CRM.WorkflowAutomation.Application.Abstractions.Rules;
using NetMetric.CRM.WorkflowAutomation.Infrastructure.Health;
using NetMetric.CRM.WorkflowAutomation.Infrastructure.Persistence;
using NetMetric.CRM.WorkflowAutomation.Infrastructure.Processing;

namespace NetMetric.CRM.WorkflowAutomation.Infrastructure.DependencyInjection;

public static class WorkflowAutomationModuleServiceCollectionExtensions
{
    public static IServiceCollection AddWorkflowAutomationModule(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("WorkflowAutomationConnection")
            ?? configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("WorkflowAutomationConnection connection string not found.");

        services.AddDbContext<WorkflowAutomationDbContext>((sp, options) =>
        {
            options.UseSqlServer(connectionString, sql => sql.MigrationsAssembly(typeof(WorkflowAutomationDbContext).Assembly.FullName));
        });

        services.AddScoped<IWorkflowAutomationDbContext>(sp => sp.GetRequiredService<WorkflowAutomationDbContext>());
        services.AddSingleton<IValidateOptions<WorkflowAutomationOptions>, WorkflowAutomationOptionsValidation>();
        services.AddOptions<WorkflowAutomationOptions>()
            .Bind(configuration.GetSection("Crm:WorkflowAutomation"))
            .ValidateOnStart();
        services.PostConfigure<WorkflowAutomationOptions>(options =>
        {
            options.EngineEnabled = configuration.GetValue("Crm:Features:WorkflowAutomationEngineEnabled", options.EngineEnabled);
            options.WorkerEnabled = configuration.GetValue("Crm:Features:WorkflowAutomationWorkerEnabled", options.WorkerEnabled);
        });
        services.AddSingleton<IWorkflowExecutionProcessingState, WorkflowExecutionProcessingState>();
        services.AddScoped<IWorkflowRuleEngine, WorkflowRuleEngine>();
        services.AddScoped<IWorkflowTriggerEvaluator, WorkflowTriggerEvaluator>();
        services.AddScoped<IWorkflowConditionEvaluator, WorkflowConditionEvaluator>();
        services.AddScoped<IWorkflowActionDispatcher, WorkflowActionDispatcher>();
        services.AddScoped<IWorkflowActionPermissionGuard, WorkflowActionPermissionGuard>();
        services.AddScoped<IWorkflowPayloadRedactor, WorkflowPayloadRedactor>();
        services.AddScoped<IWorkflowExecutionProcessor, WorkflowExecutionProcessor>();
        services.AddSingleton<IWebhookDnsResolver, SystemWebhookDnsResolver>();
        services.AddSingleton<WebhookOutboundRequestValidator>();
        services.AddSingleton(sp => new HttpClient(
            WebhookHttpClientHandlerFactory.Create(
                sp.GetRequiredService<IOptions<WorkflowAutomationOptions>>().Value.StrictWebhookConnectionPinning),
            disposeHandler: true)
        {
            Timeout = Timeout.InfiniteTimeSpan
        });
        services.AddHostedService<WorkflowExecutionBackgroundService>();
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(IWorkflowAutomationDbContext).Assembly));
        services.AddValidatorsFromAssembly(typeof(IWorkflowAutomationDbContext).Assembly);

        services.AddHealthChecks()
            .AddCheck<WorkflowAutomationDbContextHealthCheck>(
                name: "workflowautomation-db",
                failureStatus: HealthStatus.Unhealthy,
                tags: ["ready", "db", "workflowautomation"])
            .AddCheck<WorkflowAutomationWorkerHealthCheck>(
                name: "workflowautomation-worker",
                failureStatus: HealthStatus.Degraded,
                tags: ["ready", "worker", "workflowautomation"]);

        return services;
    }
}
