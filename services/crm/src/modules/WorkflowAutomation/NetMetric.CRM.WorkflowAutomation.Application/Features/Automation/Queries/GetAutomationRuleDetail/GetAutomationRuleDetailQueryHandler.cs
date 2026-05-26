// <copyright file="GetAutomationRuleDetailQueryHandler.cs" company="NetMetric">
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

namespace NetMetric.CRM.WorkflowAutomation.Application.Features.Automation.Queries.GetAutomationRuleDetail;

public sealed class GetAutomationRuleDetailQueryHandler(
    IWorkflowAutomationDbContext dbContext,
    ITenantContext tenantContext) : IRequestHandler<GetAutomationRuleDetailQuery, WorkflowRuleDetailDto>
{
    public async Task<WorkflowRuleDetailDto> Handle(GetAutomationRuleDetailQuery request, CancellationToken cancellationToken)
    {
        var tenantId = TenantRequestGuard.Resolve(tenantContext, request.TenantId);
        var rule = await dbContext.AutomationRules
            .AsNoTracking()
            .Include(x => x.Versions)
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == request.RuleId, cancellationToken)
            ?? throw new NotFoundAppException("Workflow automation rule not found.");

        return new WorkflowRuleDetailDto(
            rule.Id,
            rule.Name,
            rule.Description,
            rule.TriggerType,
            rule.EntityType,
            rule.TriggerDefinitionJson,
            rule.ConditionDefinitionJson,
            rule.ActionDefinitionJson,
            rule.Version,
            rule.IsActive,
            rule.Priority,
            rule.MaxAttempts,
            rule.TenantDailyExecutionLimit,
            rule.LoopPreventionWindowSeconds,
            rule.MaxLoopDepth,
            rule.ActivatedAtUtc,
            rule.DeactivatedAtUtc,
            rule.LastTriggeredAtUtc,
            rule.LastExecutionStatus,
            rule.NextRunAtUtc,
            rule.ScheduleCron,
            rule.ScheduleIntervalSeconds,
            rule.Versions
                .OrderByDescending(x => x.Version)
                .Select(x => new WorkflowRuleVersionDto(x.Id, x.Version, x.ChangeReason, x.ChangedBy, x.CreatedAt))
                .ToList());
    }
}
