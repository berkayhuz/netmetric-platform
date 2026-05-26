// <copyright file="ListExecutionLogsQueryHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.WorkflowAutomation.Application.Abstractions.Persistence;
using NetMetric.CRM.WorkflowAutomation.Application.Security;
using NetMetric.CRM.WorkflowAutomation.Contracts.DTOs;
using NetMetric.CRM.WorkflowAutomation.Domain.Entities.RuleExecutionLogs;
using NetMetric.Pagination;
using NetMetric.Tenancy;

namespace NetMetric.CRM.WorkflowAutomation.Application.Features.Automation.Queries.ListExecutionLogs;

public sealed class ListExecutionLogsQueryHandler(
    IWorkflowAutomationDbContext dbContext,
    ITenantContext tenantContext) : IRequestHandler<ListExecutionLogsQuery, PagedResult<WorkflowExecutionLogListItemDto>>
{
    public async Task<PagedResult<WorkflowExecutionLogListItemDto>> Handle(ListExecutionLogsQuery request, CancellationToken cancellationToken)
    {
        var tenantId = TenantRequestGuard.Resolve(tenantContext, request.TenantId);
        var page = PageRequest.Normalize(request.Page, request.PageSize);
        var query = dbContext.RuleExecutionLogs.AsNoTracking().Where(x => x.TenantId == tenantId);

        if (request.RuleId.HasValue)
        {
            query = query.Where(x => x.RuleId == request.RuleId.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            var status = request.Status.Trim();
            query = query.Where(x => x.Status == status);
        }

        if (request.FailedOnly == true)
        {
            query = query.Where(x => x.Status == WorkflowExecutionStatuses.DeadLettered || x.Status == WorkflowExecutionStatuses.PermissionDenied);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(x => x.ScheduledAtUtc)
            .ThenByDescending(x => x.CreatedAt)
            .Skip(page.Skip)
            .Take(page.PageSize)
            .Select(x => new WorkflowExecutionLogListItemDto(
                x.Id,
                x.RuleId,
                x.RuleName,
                x.RuleVersion,
                x.TriggerType,
                x.EntityType,
                x.EntityId,
                x.Status,
                x.IsDryRun,
                x.AttemptNumber,
                x.MaxAttempts,
                x.ScheduledAtUtc,
                x.NextAttemptAtUtc,
                x.StartedAtUtc,
                x.CompletedAtUtc,
                x.DeadLetteredAtUtc,
                x.ErrorClassification,
                x.ErrorCode,
                x.ErrorMessage,
                x.CorrelationId))
            .ToListAsync(cancellationToken);

        return PagedResult<WorkflowExecutionLogListItemDto>.Create(items, totalCount, page);
    }
}
