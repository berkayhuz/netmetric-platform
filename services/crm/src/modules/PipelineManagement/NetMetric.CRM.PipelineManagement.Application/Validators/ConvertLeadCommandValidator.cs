// <copyright file="ConvertLeadCommandValidator.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentValidation;
using NetMetric.CRM.PipelineManagement.Application.Commands;

namespace NetMetric.CRM.PipelineManagement.Application.Validators;

public sealed class ConvertLeadCommandValidator : AbstractValidator<ConvertLeadCommand>
{
    public ConvertLeadCommandValidator()
    {
        RuleFor(x => x.LeadId).NotEmpty();
        RuleFor(x => x).Must(x => x.CreateCustomer || x.ExistingCustomerId.HasValue)
            .When(x => x.CreateOpportunity)
            .WithMessage("Creating an opportunity requires a target customer.");
        RuleFor(x => x.OpportunityName)
            .NotEmpty()
            .When(x => x.CreateOpportunity)
            .MaximumLength(256);
    }
}
