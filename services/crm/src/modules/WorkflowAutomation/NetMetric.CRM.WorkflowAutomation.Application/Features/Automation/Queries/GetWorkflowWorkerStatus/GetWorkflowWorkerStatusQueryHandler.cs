// <copyright file="GetWorkflowWorkerStatusQueryHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.WorkflowAutomation.Application.Abstractions.Persistence;
using NetMetric.CRM.WorkflowAutomation.Application.Abstractions.Rules;
using NetMetric.CRM.WorkflowAutomation.Application.Security;
using NetMetric.CRM.WorkflowAutomation.Contracts.DTOs;
using NetMetric.CRM.WorkflowAutomation.Domain.Entities.RuleExecutionLogs;
using NetMetric.Tenancy;

namespace NetMetric.CRM.WorkflowAutomation.Application.Features.Automation.Queries.GetWorkflowWorkerStatus;

public sealed class GetWorkflowWorkerStatusQueryHandler(
    IWorkflowAutomationDbContext dbContext,
    ITenantContext tenantContext,
    IWorkflowExecutionProcessingState processingState) : IRequestHandler<GetWorkflowWorkerStatusQuery, WorkflowWorkerStatusDto>
{
    public async Task<WorkflowWorkerStatusDto> Handle(GetWorkflowWorkerStatusQuery request, CancellationToken cancellationToken)
    {
        var tenantId = TenantRequestGuard.Resolve(tenantContext, request.TenantId);
        var now = DateTime.UtcNow;
        var query = dbContext.RuleExecutionLogs.AsNoTracking().Where(x => x.TenantId == tenantId);

        var due = await query.CountAsync(
            x => (x.Status == WorkflowExecutionStatuses.Queued || x.Status == WorkflowExecutionStatuses.Retrying) &&
                 x.ScheduledAtUtc <= now &&
                 (x.NextAttemptAtUtc == null || x.NextAttemptAtUtc <= now),
            cancellationToken);
        var processing = await query.CountAsync(x => x.Status == WorkflowExecutionStatuses.Processing, cancellationToken);
        var retrying = await query.CountAsync(x => x.Status == WorkflowExecutionStatuses.Retrying, cancellationToken);
        var deadLettered = await query.CountAsync(x => x.Status == WorkflowExecutionStatuses.DeadLettered, cancellationToken);

        return new WorkflowWorkerStatusDto(processingState.IsEnabled, due, processing, retrying, deadLettered, now);
    }
}
