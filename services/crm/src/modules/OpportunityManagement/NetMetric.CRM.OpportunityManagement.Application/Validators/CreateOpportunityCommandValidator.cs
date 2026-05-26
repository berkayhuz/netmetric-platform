// <copyright file="CreateOpportunityCommandValidator.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentValidation;
using NetMetric.CRM.OpportunityManagement.Application.Commands;

namespace NetMetric.CRM.OpportunityManagement.Application.Validators;

public sealed class CreateOpportunityCommandValidator : AbstractValidator<CreateOpportunityCommand>
{
    public CreateOpportunityCommandValidator()
    {
        RuleFor(x => x.OpportunityCode).NotEmpty().MaximumLength(64);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.EstimatedAmount).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Probability).InclusiveBetween(0, 100);
    }
}
