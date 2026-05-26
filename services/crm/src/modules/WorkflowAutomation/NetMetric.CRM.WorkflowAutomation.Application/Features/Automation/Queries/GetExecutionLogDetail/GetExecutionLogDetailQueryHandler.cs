// <copyright file="GetExecutionLogDetailQueryHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.WorkflowAutomation.Application.Abstractions.Persistence;
using NetMetric.CRM.WorkflowAutomation.Application.Security;
using NetMetric.CRM.WorkflowAutomation.Contracts.DTOs;
using NetMetric.Exceptions;
using NetMetric.Tenancy;

namespace NetMetric.CRM.WorkflowAutomation.Application.Features.Automation.Queries.GetExecutionLogDetail;

public sealed class GetExecutionLogDetailQueryHandler(
    IWorkflowAutomationDbContext dbContext,
    ITenantContext tenantContext) : IRequestHandler<GetExecutionLogDetailQuery, WorkflowExecutionLogDetailDto>
{
    public async Task<WorkflowExecutionLogDetailDto> Handle(GetExecutionLogDetailQuery request, CancellationToken cancellationToken)
    {
        var tenantId = TenantRequestGuard.Resolve(tenantContext, request.TenantId);
        var execution = await dbContext.RuleExecutionLogs
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == request.ExecutionLogId, cancellationToken)
            ?? throw new NotFoundAppException("Workflow execution log not found.");

        var deliveries = await dbContext.WebhookDeliveryLogs
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.ExecutionLogId == execution.Id)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new WorkflowWebhookDeliveryDto(
                x.Id,
                x.EventKey,
                x.TargetUrl,
                x.Status,
                x.AttemptNumber,
                x.MaxAttempts,
                x.HttpStatusCode,
                x.ResponseSnippet,
                x.ErrorMessage,
                x.DeliveredAtUtc,
                x.NextAttemptAtUtc,
                x.CorrelationId))
            .ToListAsync(cancellationToken);

        return new WorkflowExecutionLogDetailDto(
            execution.Id,
            execution.RuleId,
            execution.RuleName,
            execution.RuleVersion,
            execution.TriggerType,
            execution.EntityType,
            execution.EntityId,
            execution.Status,
            execution.IsDryRun,
            execution.AttemptNumber,
            execution.MaxAttempts,
            execution.IdempotencyKey,
            execution.CorrelationId,
            execution.LoopFingerprint,
            execution.LoopDepth,
            execution.ScheduledAtUtc,
            execution.NextAttemptAtUtc,
            execution.StartedAtUtc,
            execution.CompletedAtUtc,
            execution.DeadLetteredAtUtc,
            execution.ErrorClassification,
            execution.ErrorCode,
            execution.ErrorMessage,
            execution.TriggerPayloadJson,
            execution.ConditionResultJson,
            execution.ActionResultJson,
            deliveries);
    }
}
