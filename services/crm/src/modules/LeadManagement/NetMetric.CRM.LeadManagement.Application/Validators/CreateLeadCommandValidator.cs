// <copyright file="CreateLeadCommandValidator.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentValidation;
using NetMetric.CRM.LeadManagement.Application.Commands.Leads;

namespace NetMetric.CRM.LeadManagement.Application.Validators;

public sealed class CreateLeadCommandValidator : AbstractValidator<CreateLeadCommand>
{
    public CreateLeadCommandValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.CompanyName).MaximumLength(200);
        RuleFor(x => x.Email).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email));
        RuleFor(x => x.Phone).MaximumLength(50);
        RuleFor(x => x.JobTitle).MaximumLength(128);
        RuleFor(x => x.EstimatedBudget).GreaterThanOrEqualTo(0).When(x => x.EstimatedBudget.HasValue);
        RuleFor(x => x.NextContactDate)
            .Must(x => !x.HasValue || x.Value > DateTime.UtcNow.AddYears(-10))
            .WithMessage("Next contact date is invalid.");
    }
}
