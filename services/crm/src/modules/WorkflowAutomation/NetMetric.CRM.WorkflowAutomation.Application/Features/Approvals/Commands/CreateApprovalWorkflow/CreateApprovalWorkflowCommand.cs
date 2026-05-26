// <copyright file="CreateApprovalWorkflowCommand.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;

namespace NetMetric.CRM.WorkflowAutomation.Application.Features.Approvals.Commands.CreateApprovalWorkflow;

public sealed record CreateApprovalWorkflowCommand(
    string Name,
    string EntityType,
    Guid? RelatedEntityId = null,
    string? RoutingPolicyJson = null,
    string? EscalationPolicyJson = null,
    string? SlaPolicyJson = null) : IRequest<Guid>;
