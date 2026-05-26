// <copyright file="ConvertLeadToCustomerCommandValidator.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentValidation;
using NetMetric.CRM.LeadManagement.Application.Features.Conversions.Commands.ConvertLeadToCustomer;

namespace NetMetric.CRM.LeadManagement.Application.Validators;

public sealed class ConvertLeadToCustomerCommandValidator : AbstractValidator<ConvertLeadToCustomerCommand>
{
    public ConvertLeadToCustomerCommandValidator()
    {
        RuleFor(x => x.LeadId).NotEmpty();
        RuleFor(x => x.OpportunityName)
            .MaximumLength(200)
            .When(x => !string.IsNullOrWhiteSpace(x.OpportunityName));

        RuleFor(x => x.EstimatedAmount)
            .GreaterThanOrEqualTo(0)
            .When(x => x.EstimatedAmount.HasValue);
    }
}
