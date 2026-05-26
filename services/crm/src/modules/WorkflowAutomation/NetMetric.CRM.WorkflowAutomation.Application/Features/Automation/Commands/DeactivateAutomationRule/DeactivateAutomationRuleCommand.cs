// <copyright file="DeactivateAutomationRuleCommand.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;

namespace NetMetric.CRM.WorkflowAutomation.Application.Features.Automation.Commands.DeactivateAutomationRule;

public sealed record DeactivateAutomationRuleCommand(Guid TenantId, Guid RuleId) : IRequest;
