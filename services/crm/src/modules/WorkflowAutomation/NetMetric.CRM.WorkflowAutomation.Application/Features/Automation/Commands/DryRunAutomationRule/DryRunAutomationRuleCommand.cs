// <copyright file="DryRunAutomationRuleCommand.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.WorkflowAutomation.Contracts.DTOs;

namespace NetMetric.CRM.WorkflowAutomation.Application.Features.Automation.Commands.DryRunAutomationRule;

public sealed record DryRunAutomationRuleCommand(
    Guid TenantId,
    string TriggerType,
    string EntityType,
    Guid? EntityId,
    string PayloadJson,
    Guid? RuleId = null,
    string? CorrelationId = null) : IRequest<WorkflowRuleExecutionResultDto>;
