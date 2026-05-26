// <copyright file="UpdateOpportunityCommandValidator.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentValidation;
using NetMetric.CRM.OpportunityManagement.Application.Commands;

namespace NetMetric.CRM.OpportunityManagement.Application.Validators;

public sealed class UpdateOpportunityCommandValidator : AbstractValidator<UpdateOpportunityCommand>
{
    public UpdateOpportunityCommandValidator()
    {
        RuleFor(x => x.OpportunityId).NotEmpty();
        RuleFor(x => x.OpportunityCode).NotEmpty().MaximumLength(64);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.RowVersion).NotEmpty();
    }
}
