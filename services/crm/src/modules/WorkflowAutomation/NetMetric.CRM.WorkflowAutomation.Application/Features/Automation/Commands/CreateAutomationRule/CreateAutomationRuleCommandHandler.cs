// <copyright file="CreateAutomationRuleCommandHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.WorkflowAutomation.Application.Abstractions.Persistence;
using NetMetric.CRM.WorkflowAutomation.Domain.Entities.AutomationRules;
using NetMetric.CurrentUser;

namespace NetMetric.CRM.WorkflowAutomation.Application.Features.Automation.Commands.CreateAutomationRule;

public sealed class CreateAutomationRuleCommandHandler(
    IWorkflowAutomationDbContext dbContext,
    ICurrentUserService currentUserService) : IRequestHandler<CreateAutomationRuleCommand, Guid>
{
    public async Task<Guid> Handle(CreateAutomationRuleCommand request, CancellationToken cancellationToken)
    {
        currentUserService.EnsureTenant();

        var rule = AutomationRule.Create(
            request.Name,
            request.TriggerType,
            request.EntityType,
            request.TriggerDefinitionJson,
            request.ConditionDefinitionJson,
            request.ActionDefinitionJson,
            request.Description,
            request.Priority,
            request.MaxAttempts,
            request.TenantDailyExecutionLimit,
            request.LoopPreventionWindowSeconds,
            request.MaxLoopDepth,
            request.IsActive,
            request.NextRunAtUtc,
            request.ScheduleCron,
            request.ScheduleIntervalSeconds,
            request.TemplateKey,
            currentUserService.UserName);

        await dbContext.AutomationRules.AddAsync(rule, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return rule.Id;
    }
}
