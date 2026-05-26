// <copyright file="WorkflowExecutionBackgroundService.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NetMetric.CRM.WorkflowAutomation.Application.Abstractions.Rules;

namespace NetMetric.CRM.WorkflowAutomation.Infrastructure.Processing;

public sealed class WorkflowExecutionBackgroundService(
    IServiceScopeFactory scopeFactory,
    IOptions<WorkflowAutomationOptions> options,
    ILogger<WorkflowExecutionBackgroundService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var configured = options.Value;
        if (!configured.WorkerEnabled)
        {
            logger.LogInformation("Workflow automation worker is disabled by Crm:Features:WorkflowAutomationWorkerEnabled.");
            return;
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var processor = scope.ServiceProvider.GetRequiredService<IWorkflowExecutionProcessor>();
                var processed = await processor.ProcessDueExecutionsAsync(stoppingToken);
                if (processed > 0)
                {
                    logger.LogInformation("Workflow automation worker processed {ProcessedExecutionCount} executions.", processed);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Workflow automation worker cycle failed.");
            }

            await Task.Delay(configured.PollInterval, stoppingToken);
        }
    }
}
