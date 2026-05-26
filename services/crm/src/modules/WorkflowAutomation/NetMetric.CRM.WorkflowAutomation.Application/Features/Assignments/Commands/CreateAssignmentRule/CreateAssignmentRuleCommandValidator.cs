// <copyright file="CreateAssignmentRuleCommandValidator.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentValidation;

namespace NetMetric.CRM.WorkflowAutomation.Application.Features.Assignments.Commands.CreateAssignmentRule;

public sealed class CreateAssignmentRuleCommandValidator : AbstractValidator<CreateAssignmentRuleCommand>
{
    public CreateAssignmentRuleCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.EntityType).NotEmpty().MaximumLength(100);
        RuleFor(x => x.ConditionJson).NotEmpty().MaximumLength(100_000);
        RuleFor(x => x.AssigneeSelectorJson).MaximumLength(100_000);
        RuleFor(x => x.FallbackAssigneeJson).MaximumLength(100_000);
        RuleFor(x => x.Priority).InclusiveBetween(0, 1000);
    }
}
