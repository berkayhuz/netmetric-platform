// <copyright file="UpdateCompanyCommandValidator.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentValidation;
using NetMetric.CRM.CustomerManagement.Application.Commands.Companies;

namespace NetMetric.CRM.CustomerManagement.Application.Validators;

public sealed class UpdateCompanyCommandValidator : AbstractValidator<UpdateCompanyCommand>
{
    public UpdateCompanyCommandValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Email).MaximumLength(200).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email));
        RuleFor(x => x.Website).MaximumLength(500);
        RuleFor(x => x.Phone).MaximumLength(50);
        RuleFor(x => x.TaxNumber).MaximumLength(50);
        RuleFor(x => x.TaxOffice).MaximumLength(100);
        RuleFor(x => x.Sector).MaximumLength(100);
        RuleFor(x => x.EmployeeCountRange).MaximumLength(100);
        RuleFor(x => x.Description).MaximumLength(2000);
        RuleFor(x => x.Notes).MaximumLength(4000);
    }
}
