// <copyright file="DryRunAutomationRuleCommandHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.WorkflowAutomation.Application.Abstractions.Rules;
using NetMetric.CRM.WorkflowAutomation.Application.Security;
using NetMetric.CRM.WorkflowAutomation.Contracts.DTOs;
using NetMetric.Tenancy;

namespace NetMetric.CRM.WorkflowAutomation.Application.Features.Automation.Commands.DryRunAutomationRule;

public sealed class DryRunAutomationRuleCommandHandler(
    IWorkflowRuleEngine ruleEngine,
    ITenantContext tenantContext) : IRequestHandler<DryRunAutomationRuleCommand, WorkflowRuleExecutionResultDto>
{
    public Task<WorkflowRuleExecutionResultDto> Handle(DryRunAutomationRuleCommand request, CancellationToken cancellationToken)
    {
        var tenantId = TenantRequestGuard.Resolve(tenantContext, request.TenantId);
        return ruleEngine.DryRunAsync(
            new WorkflowRuleExecutionRequest(
                tenantId,
                request.TriggerType,
                request.EntityType,
                request.EntityId,
                request.PayloadJson,
                CorrelationId: request.CorrelationId,
                RuleId: request.RuleId),
            cancellationToken);
    }
}
