// <copyright file="CreateSmartLabelRuleCommandValidator.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentValidation;

namespace NetMetric.CRM.TagManagement.Application.Features.SmartLabels.Commands.CreateSmartLabelRule;

public sealed class CreateSmartLabelRuleCommandValidator : AbstractValidator<CreateSmartLabelRuleCommand>
{
    public CreateSmartLabelRuleCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.EntityType).NotEmpty().MaximumLength(200);
        RuleFor(x => x.ConditionJson).NotEmpty().MaximumLength(200);
    }
}
