// <copyright file="WorkflowExecutionProcessor.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NetMetric.CRM.WorkflowAutomation.Application.Abstractions.Rules;
using NetMetric.CRM.WorkflowAutomation.Domain.Entities.RuleExecutionLogs;
using NetMetric.CRM.WorkflowAutomation.Infrastructure.Persistence;

namespace NetMetric.CRM.WorkflowAutomation.Infrastructure.Processing;

public sealed class WorkflowExecutionProcessor(
    WorkflowAutomationDbContext dbContext,
    IWorkflowRuleEngine ruleEngine,
    IOptions<WorkflowAutomationOptions> options,
    ILogger<WorkflowExecutionProcessor> logger) : IWorkflowExecutionProcessor
{
    private readonly WorkflowAutomationOptions _options = options.Value;
    private readonly string _workerId = $"{Environment.MachineName}:{Guid.NewGuid():N}";

    public async Task<int> ProcessDueExecutionsAsync(CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var batchSize = Math.Clamp(_options.BatchSize, 1, 100);
        var dueExecutions = await dbContext.RuleExecutionLogs
            .IgnoreQueryFilters()
            .Where(x =>
                (x.Status == WorkflowExecutionStatuses.Queued || x.Status == WorkflowExecutionStatuses.Retrying) &&
                x.ScheduledAtUtc <= now &&
                (x.NextAttemptAtUtc == null || x.NextAttemptAtUtc <= now))
            .OrderBy(x => x.NextAttemptAtUtc ?? x.ScheduledAtUtc)
            .ThenBy(x => x.CreatedAt)
            .Take(batchSize)
            .ToListAsync(cancellationToken);

        var processed = 0;
        foreach (var execution in dueExecutions)
        {
            if (!execution.TryAcquire(_workerId, now, _options.LeaseDuration))
            {
                continue;
            }

            await dbContext.SaveChangesAsync(cancellationToken);
            try
            {
                await ruleEngine.ExecuteQueuedAsync(execution.Id, cancellationToken);
                processed += 1;
            }
            catch (Exception exception) when (exception is not OperationCanceledException || !cancellationToken.IsCancellationRequested)
            {
                logger.LogError(exception, "Workflow worker failed while processing execution {ExecutionId}.", execution.Id);
            }
        }

        return processed;
    }
}
