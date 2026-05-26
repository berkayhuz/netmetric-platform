// <copyright file="AddCompanyAddressCommandValidator.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentValidation;
using NetMetric.CRM.CustomerManagement.Application.Commands.Addresses;

namespace NetMetric.CRM.CustomerManagement.Application.Validators;

public sealed class AddCompanyAddressCommandValidator : AbstractValidator<AddCompanyAddressCommand>
{
    public AddCompanyAddressCommandValidator()
    {
        RuleFor(x => x.CompanyId).NotEmpty();
        RuleFor(x => x.Line1).NotEmpty().MaximumLength(250);
        RuleFor(x => x.Line2).MaximumLength(250);
        RuleFor(x => x.District).MaximumLength(100);
        RuleFor(x => x.City).MaximumLength(100);
        RuleFor(x => x.State).MaximumLength(100);
        RuleFor(x => x.Country).MaximumLength(100);
        RuleFor(x => x.ZipCode).MaximumLength(25);
    }
}
