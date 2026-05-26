// <copyright file="WorkflowRuleEngine.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NetMetric.CRM.WorkflowAutomation.Application.Abstractions.Rules;
using NetMetric.CRM.WorkflowAutomation.Contracts.DTOs;
using NetMetric.CRM.WorkflowAutomation.Domain.Entities.AutomationRules;
using NetMetric.CRM.WorkflowAutomation.Domain.Entities.RuleExecutionLogs;
using NetMetric.CRM.WorkflowAutomation.Infrastructure.Persistence;
using NetMetric.CurrentUser;

namespace NetMetric.CRM.WorkflowAutomation.Infrastructure.Processing;

public sealed class WorkflowRuleEngine(
    WorkflowAutomationDbContext dbContext,
    IWorkflowTriggerEvaluator triggerEvaluator,
    IWorkflowConditionEvaluator conditionEvaluator,
    IWorkflowActionDispatcher actionDispatcher,
    IWorkflowPayloadRedactor payloadRedactor,
    ICurrentUserService currentUserService,
    IOptions<WorkflowAutomationOptions> options,
    ILogger<WorkflowRuleEngine> logger) : IWorkflowRuleEngine
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);
    private readonly WorkflowAutomationOptions _options = options.Value;

    public Task<WorkflowRuleExecutionResultDto> ExecuteAsync(WorkflowRuleExecutionRequest request, CancellationToken cancellationToken)
        => ExecuteInternalAsync(request, dryRun: false, cancellationToken);

    public Task<WorkflowRuleExecutionResultDto> DryRunAsync(WorkflowRuleExecutionRequest request, CancellationToken cancellationToken)
        => ExecuteInternalAsync(request, dryRun: true, cancellationToken);

    public async Task<WorkflowRuleExecutionResultDto> ExecuteQueuedAsync(Guid executionLogId, CancellationToken cancellationToken)
    {
        var execution = await dbContext.RuleExecutionLogs
            .IgnoreQueryFilters()
            .FirstAsync(x => x.Id == executionLogId, cancellationToken);

        var rule = await dbContext.AutomationRules
            .IgnoreQueryFilters()
            .FirstAsync(x => x.TenantId == execution.TenantId && x.Id == execution.RuleId, cancellationToken);

        return await ExecuteAcquiredLogAsync(rule, execution, execution.TriggerPayloadJson, execution.PermissionSnapshotJson, cancellationToken);
    }

    private async Task<WorkflowRuleExecutionResultDto> ExecuteInternalAsync(WorkflowRuleExecutionRequest request, bool dryRun, CancellationToken cancellationToken)
    {
        if (!_options.EngineEnabled)
        {
            throw new InvalidOperationException("Workflow automation engine is disabled by configuration.");
        }

        var rules = await dbContext.AutomationRules
            .Include(x => x.Versions)
            .Where(x =>
                x.TenantId == request.TenantId &&
                x.IsActive &&
                (!request.RuleId.HasValue || x.Id == request.RuleId.Value))
            .OrderBy(x => x.Priority)
            .ThenBy(x => x.Name)
            .ToListAsync(cancellationToken);

        var simulations = new List<WorkflowRuleSimulationDto>();
        Guid? firstExecutionId = null;
        var executedActions = 0;

        foreach (var rule in rules)
        {
            var triggerMatched = triggerEvaluator.IsMatch(rule, request);
            if (!triggerMatched)
            {
                continue;
            }

            var conditionResult = conditionEvaluator.Evaluate(rule.ConditionDefinitionJson, request.PayloadJson);
            var loopFingerprint = WorkflowExecutionPolicy.ComputeLoopFingerprint(request.TenantId, rule, request.TriggerType, request.EntityType, request.EntityId);
            if (!dryRun && conditionResult.Matched)
            {
                var existingCompleted = await dbContext.RuleExecutionLogs
                    .IgnoreQueryFilters()
                    .AsNoTracking()
                    .FirstOrDefaultAsync(
                        x => x.TenantId == request.TenantId &&
                             x.IdempotencyKey == WorkflowExecutionPolicy.ComputeIdempotencyKey(request.TenantId, rule, request.TriggerType, request.EntityType, request.EntityId, request.PayloadJson, request.IdempotencyKey) &&
                             x.Status == WorkflowExecutionStatuses.Completed,
                        cancellationToken);
                if (existingCompleted is not null)
                {
                    simulations.Add(new WorkflowRuleSimulationDto(rule.Id, rule.Name, rule.Version, true, true, false, conditionResult.ResultJson, [
                        new WorkflowActionSimulationDto("idempotency", WorkflowExecutionStatuses.IdempotentSkip, "A completed execution already exists for this idempotency key.", false, null)
                    ]));
                    firstExecutionId ??= existingCompleted.Id;
                    WorkflowAutomationMetrics.RecordSkipped(rule.TriggerType, WorkflowExecutionStatuses.IdempotentSkip);
                    continue;
                }
            }

            var loopPrevented = await ShouldPreventLoopAsync(rule, loopFingerprint, request.LoopDepth, cancellationToken);

            if (dryRun)
            {
                var dryRunDispatch = conditionResult.Matched && !loopPrevented
                    ? await actionDispatcher.DispatchAsync(CreateDispatchContext(rule, request, Guid.Empty, request.PayloadJson, dryRun: true, "[]"), cancellationToken)
                    : new WorkflowActionDispatchResult(0, "[]", []);

                var dryRunLog = RuleExecutionLog.DryRun(
                    request.TenantId,
                    rule.Id,
                    rule.Version,
                    rule.Name,
                    request.TriggerType,
                    request.EntityType,
                    request.EntityId,
                    request.CorrelationId ?? CreateCorrelationId(request.TenantId, rule.Id),
                    loopFingerprint,
                    payloadRedactor.RedactJson(request.PayloadJson),
                    conditionResult.ResultJson,
                    dryRunDispatch.ActionResultJson,
                    currentUserService.UserId == Guid.Empty ? null : currentUserService.UserId);

                await dbContext.RuleExecutionLogs.AddAsync(dryRunLog, cancellationToken);
                simulations.Add(new WorkflowRuleSimulationDto(rule.Id, rule.Name, rule.Version, true, conditionResult.Matched, loopPrevented, conditionResult.ResultJson, dryRunDispatch.Actions));
                continue;
            }

            if (!conditionResult.Matched || loopPrevented || await HasExceededTenantLimitAsync(rule, cancellationToken))
            {
                var skipped = await QueueExecutionAsync(rule, request, loopFingerprint, cancellationToken);
                skipped.MarkSkipped(
                    loopPrevented ? WorkflowExecutionStatuses.LoopPrevented : WorkflowExecutionStatuses.Skipped,
                    loopPrevented ? "Loop prevention blocked this execution." : "Conditions did not match or tenant execution limit was reached.",
                    conditionResult.ResultJson,
                    DateTime.UtcNow);
                rule.MarkTriggered(DateTime.UtcNow, skipped.Status);
                await dbContext.SaveChangesAsync(cancellationToken);
                WorkflowAutomationMetrics.RecordSkipped(rule.TriggerType, skipped.Status);
                simulations.Add(new WorkflowRuleSimulationDto(rule.Id, rule.Name, rule.Version, true, conditionResult.Matched, loopPrevented, conditionResult.ResultJson, []));
                firstExecutionId ??= skipped.Id;
                continue;
            }

            var execution = await QueueExecutionAsync(rule, request, loopFingerprint, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);

            if (!execution.TryAcquire(Environment.MachineName, DateTime.UtcNow, _options.LeaseDuration))
            {
                continue;
            }

            await dbContext.SaveChangesAsync(cancellationToken);
            var result = await ExecuteAcquiredLogAsync(rule, execution, request.PayloadJson, execution.PermissionSnapshotJson, cancellationToken);
            executedActions += result.ExecutedActions;
            simulations.AddRange(result.Rules);
            firstExecutionId ??= execution.Id;
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return new WorkflowRuleExecutionResultDto(
            firstExecutionId,
            dryRun,
            simulations.Count,
            executedActions,
            dryRun ? WorkflowExecutionStatuses.Simulated : WorkflowExecutionStatuses.Completed,
            simulations);
    }

    private async Task<WorkflowRuleExecutionResultDto> ExecuteAcquiredLogAsync(
        AutomationRule rule,
        RuleExecutionLog execution,
        string payloadJson,
        string permissionSnapshotJson,
        CancellationToken cancellationToken)
    {
        var conditionResult = conditionEvaluator.Evaluate(rule.ConditionDefinitionJson, payloadJson);
        if (!conditionResult.Matched)
        {
            execution.MarkSkipped(WorkflowExecutionStatuses.Skipped, "Conditions did not match.", conditionResult.ResultJson, DateTime.UtcNow);
            rule.MarkTriggered(DateTime.UtcNow, execution.Status);
            await dbContext.SaveChangesAsync(cancellationToken);
            WorkflowAutomationMetrics.RecordSkipped(rule.TriggerType, WorkflowExecutionStatuses.Skipped);
            return new WorkflowRuleExecutionResultDto(execution.Id, false, 1, 0, execution.Status, [
                new WorkflowRuleSimulationDto(rule.Id, rule.Name, rule.Version, true, false, false, conditionResult.ResultJson, [])
            ]);
        }

        try
        {
            var dispatch = await actionDispatcher.DispatchAsync(
                CreateDispatchContext(rule, new WorkflowRuleExecutionRequest(
                    execution.TenantId,
                    execution.TriggerType,
                    execution.EntityType,
                    execution.EntityId,
                    payloadJson,
                    execution.IdempotencyKey,
                    execution.CorrelationId,
                    execution.LoopDepth,
                    rule.Id,
                    execution.ScheduledAtUtc), execution.Id, payloadJson, dryRun: false, permissionSnapshotJson),
                cancellationToken);

            execution.MarkCompleted(conditionResult.ResultJson, dispatch.ActionResultJson, DateTime.UtcNow);
            rule.MarkTriggered(DateTime.UtcNow, execution.Status);
            await dbContext.SaveChangesAsync(cancellationToken);
            WorkflowAutomationMetrics.RecordCompleted(rule.TriggerType);

            return new WorkflowRuleExecutionResultDto(execution.Id, false, 1, dispatch.ExecutedActions, execution.Status, [
                new WorkflowRuleSimulationDto(rule.Id, rule.Name, rule.Version, true, true, false, conditionResult.ResultJson, dispatch.Actions)
            ]);
        }
        catch (Exception exception) when (exception is not OperationCanceledException || !cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning(exception, "Workflow rule {RuleId} execution {ExecutionId} failed for tenant {TenantId}.", rule.Id, execution.Id, execution.TenantId);
            await FinalizeFailureAsync(rule, execution, WorkflowErrorClassifier.Classify(exception), conditionResult.ResultJson, cancellationToken);

            return new WorkflowRuleExecutionResultDto(execution.Id, false, 1, 0, execution.Status, [
                new WorkflowRuleSimulationDto(rule.Id, rule.Name, rule.Version, true, true, false, conditionResult.ResultJson, [
                    new WorkflowActionSimulationDto("execution", execution.Status, execution.ErrorMessage ?? "Workflow execution failed.", false, null)
                ])
            ]);
        }
    }

    private async Task FinalizeFailureAsync(
        AutomationRule rule,
        RuleExecutionLog execution,
        ClassifiedWorkflowError error,
        string conditionResultJson,
        CancellationToken cancellationToken)
    {
        if (error.Classification == "authorization")
        {
            execution.MarkSkipped(WorkflowExecutionStatuses.PermissionDenied, error.SanitizedMessage, conditionResultJson, DateTime.UtcNow);
            rule.MarkTriggered(DateTime.UtcNow, execution.Status);
            await dbContext.SaveChangesAsync(cancellationToken);
            WorkflowAutomationMetrics.RecordSkipped(rule.TriggerType, WorkflowExecutionStatuses.PermissionDenied);
            return;
        }

        var maxAttempts = Math.Max(rule.MaxAttempts, Math.Max(execution.MaxAttempts, _options.MaxAttempts));
        var canRetry = error.IsRetryable && execution.AttemptNumber < maxAttempts;
        if (canRetry)
        {
            execution.MarkRetry(DateTime.UtcNow.Add(error.RetryAfter ?? WorkflowExecutionPolicy.ComputeRetryDelay(execution.AttemptNumber, _options)), error.Classification, error.ErrorCode, error.SanitizedMessage);
            WorkflowAutomationMetrics.RecordRetried(rule.TriggerType);
        }
        else
        {
            execution.MoveToDeadLetter(DateTime.UtcNow, error.Classification, error.ErrorCode, error.SanitizedMessage);
            WorkflowAutomationMetrics.RecordDeadLettered(rule.TriggerType);
        }

        rule.MarkTriggered(DateTime.UtcNow, execution.Status);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<RuleExecutionLog> QueueExecutionAsync(AutomationRule rule, WorkflowRuleExecutionRequest request, string loopFingerprint, CancellationToken cancellationToken)
    {
        var scheduledAt = request.ScheduledAtUtc ?? DateTime.UtcNow;
        var idempotencyKey = WorkflowExecutionPolicy.ComputeIdempotencyKey(request.TenantId, rule, request.TriggerType, request.EntityType, request.EntityId, request.PayloadJson, request.IdempotencyKey);
        var existing = await dbContext.RuleExecutionLogs
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.TenantId == request.TenantId && x.IdempotencyKey == idempotencyKey, cancellationToken);
        if (existing is not null)
        {
            return existing;
        }

        var execution = RuleExecutionLog.Queue(
            request.TenantId,
            rule.Id,
            rule.Version,
            rule.Name,
            request.TriggerType,
            request.EntityType,
            request.EntityId,
            idempotencyKey,
            request.CorrelationId ?? CreateCorrelationId(request.TenantId, rule.Id),
            loopFingerprint,
            request.LoopDepth,
            rule.MaxAttempts,
            scheduledAt,
            payloadRedactor.RedactJson(request.PayloadJson),
            JsonSerializer.Serialize(currentUserService.Permissions, SerializerOptions),
            currentUserService.UserId == Guid.Empty ? null : currentUserService.UserId);

        await dbContext.RuleExecutionLogs.AddAsync(execution, cancellationToken);
        return execution;
    }

    private async Task<bool> ShouldPreventLoopAsync(AutomationRule rule, string loopFingerprint, int loopDepth, CancellationToken cancellationToken)
    {
        if (loopDepth >= rule.MaxLoopDepth)
        {
            return true;
        }

        var since = DateTime.UtcNow.AddSeconds(-rule.LoopPreventionWindowSeconds);
        return await dbContext.RuleExecutionLogs
            .IgnoreQueryFilters()
            .AsNoTracking()
            .AnyAsync(
                x => x.TenantId == rule.TenantId &&
                     x.LoopFingerprint == loopFingerprint &&
                     x.StartedAtUtc >= since &&
                     x.Status != WorkflowExecutionStatuses.Simulated,
                cancellationToken);
    }

    private async Task<bool> HasExceededTenantLimitAsync(AutomationRule rule, CancellationToken cancellationToken)
    {
        var since = DateTime.UtcNow.Date;
        var count = await dbContext.RuleExecutionLogs
            .IgnoreQueryFilters()
            .AsNoTracking()
            .CountAsync(
                x => x.TenantId == rule.TenantId &&
                     x.RuleId == rule.Id &&
                     x.StartedAtUtc >= since &&
                     x.Status == WorkflowExecutionStatuses.Completed,
                cancellationToken);

        return count >= rule.TenantDailyExecutionLimit;
    }

    private static WorkflowActionDispatchContext CreateDispatchContext(
        AutomationRule rule,
        WorkflowRuleExecutionRequest request,
        Guid executionLogId,
        string payloadJson,
        bool dryRun,
        string permissionSnapshotJson)
        => new(
            rule,
            request.TenantId,
            executionLogId,
            request.TriggerType,
            request.EntityType,
            request.EntityId,
            payloadJson,
            rule.ActionDefinitionJson,
            request.CorrelationId ?? CreateCorrelationId(request.TenantId, rule.Id),
            dryRun,
            permissionSnapshotJson);

    private static string CreateCorrelationId(Guid tenantId, Guid ruleId)
        => $"{tenantId:N}-{ruleId:N}-{Guid.NewGuid():N}";
}
