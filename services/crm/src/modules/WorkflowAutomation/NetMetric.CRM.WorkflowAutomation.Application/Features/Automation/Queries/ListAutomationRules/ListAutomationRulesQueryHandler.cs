// <copyright file="ListAutomationRulesQueryHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.WorkflowAutomation.Application.Abstractions.Persistence;
using NetMetric.CRM.WorkflowAutomation.Application.Security;
using NetMetric.CRM.WorkflowAutomation.Contracts.DTOs;
using NetMetric.Pagination;
using NetMetric.Tenancy;

namespace NetMetric.CRM.WorkflowAutomation.Application.Features.Automation.Queries.ListAutomationRules;

public sealed class ListAutomationRulesQueryHandler(
    IWorkflowAutomationDbContext dbContext,
    ITenantContext tenantContext) : IRequestHandler<ListAutomationRulesQuery, PagedResult<WorkflowRuleListItemDto>>
{
    public async Task<PagedResult<WorkflowRuleListItemDto>> Handle(ListAutomationRulesQuery request, CancellationToken cancellationToken)
    {
        var tenantId = TenantRequestGuard.Resolve(tenantContext, request.TenantId);
        var page = PageRequest.Normalize(request.Page, request.PageSize);
        var query = dbContext.AutomationRules.AsNoTracking().Where(x => x.TenantId == tenantId);

        if (!string.IsNullOrWhiteSpace(request.TriggerType))
        {
            var triggerType = request.TriggerType.Trim();
            query = query.Where(x => x.TriggerType == triggerType);
        }

        if (!string.IsNullOrWhiteSpace(request.EntityType))
        {
            var entityType = request.EntityType.Trim();
            query = query.Where(x => x.EntityType == entityType);
        }

        if (request.IsActive.HasValue)
        {
            query = query.Where(x => x.IsActive == request.IsActive.Value);
        }

        query = (request.SortBy?.Trim().ToLowerInvariant(), request.SortDirection?.Trim().ToLowerInvariant()) switch
        {
            ("name", "asc") => query.OrderBy(x => x.Name),
            ("name", _) => query.OrderByDescending(x => x.Name),
            ("priority", "asc") => query.OrderBy(x => x.Priority).ThenBy(x => x.Name),
            ("priority", _) => query.OrderByDescending(x => x.Priority).ThenBy(x => x.Name),
            ("trigger", "asc") => query.OrderBy(x => x.TriggerType).ThenBy(x => x.Name),
            ("trigger", _) => query.OrderByDescending(x => x.TriggerType).ThenBy(x => x.Name),
            _ => query.OrderByDescending(x => x.UpdatedAt ?? x.CreatedAt).ThenBy(x => x.Name)
        };

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip(page.Skip)
            .Take(page.PageSize)
            .Select(x => new WorkflowRuleListItemDto(
                x.Id,
                x.Name,
                x.TriggerType,
                x.EntityType,
                x.Version,
                x.IsActive,
                x.Priority,
                x.LastTriggeredAtUtc,
                x.LastExecutionStatus,
                x.NextRunAtUtc))
            .ToListAsync(cancellationToken);

        return PagedResult<WorkflowRuleListItemDto>.Create(items, totalCount, page);
    }
}
