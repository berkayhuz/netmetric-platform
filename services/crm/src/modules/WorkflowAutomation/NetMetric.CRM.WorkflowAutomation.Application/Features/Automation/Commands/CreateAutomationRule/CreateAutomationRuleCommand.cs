// <copyright file="CreateAutomationRuleCommand.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;

namespace NetMetric.CRM.WorkflowAutomation.Application.Features.Automation.Commands.CreateAutomationRule;

public sealed record CreateAutomationRuleCommand(
    string Name,
    string TriggerType,
    string EntityType,
    string TriggerDefinitionJson,
    string ConditionDefinitionJson,
    string ActionDefinitionJson,
    string? Description = null,
    int Priority = 100,
    int MaxAttempts = 3,
    int TenantDailyExecutionLimit = 1000,
    int LoopPreventionWindowSeconds = 300,
    int MaxLoopDepth = 4,
    bool IsActive = false,
    DateTime? NextRunAtUtc = null,
    string? ScheduleCron = null,
    int? ScheduleIntervalSeconds = null,
    string? TemplateKey = null) : IRequest<Guid>;
