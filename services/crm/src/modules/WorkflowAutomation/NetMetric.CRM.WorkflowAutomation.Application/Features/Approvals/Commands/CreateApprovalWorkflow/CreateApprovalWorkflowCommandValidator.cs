// <copyright file="CreateApprovalWorkflowCommandValidator.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentValidation;

namespace NetMetric.CRM.WorkflowAutomation.Application.Features.Approvals.Commands.CreateApprovalWorkflow;

public sealed class CreateApprovalWorkflowCommandValidator : AbstractValidator<CreateApprovalWorkflowCommand>
{
    public CreateApprovalWorkflowCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.EntityType).NotEmpty().MaximumLength(100);
        RuleFor(x => x.RoutingPolicyJson).MaximumLength(100_000);
        RuleFor(x => x.EscalationPolicyJson).MaximumLength(100_000);
        RuleFor(x => x.SlaPolicyJson).MaximumLength(100_000);
    }
}
