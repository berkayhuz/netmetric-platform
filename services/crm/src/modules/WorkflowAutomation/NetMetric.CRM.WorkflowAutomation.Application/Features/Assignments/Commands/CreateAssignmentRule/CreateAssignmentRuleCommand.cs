// <copyright file="CreateAssignmentRuleCommand.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;

namespace NetMetric.CRM.WorkflowAutomation.Application.Features.Assignments.Commands.CreateAssignmentRule;

public sealed record CreateAssignmentRuleCommand(
    string Name,
    string EntityType,
    string ConditionJson,
    string? AssigneeSelectorJson = null,
    string? FallbackAssigneeJson = null,
    int Priority = 100) : IRequest<Guid>;
