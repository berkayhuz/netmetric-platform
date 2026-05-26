// <copyright file="UpdateCustomerCommandValidator.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentValidation;
using NetMetric.CRM.CustomerManagement.Application.Commands.Customers;

namespace NetMetric.CRM.CustomerManagement.Application.Validators;

public sealed class UpdateCustomerCommandValidator : AbstractValidator<UpdateCustomerCommand>
{
    public UpdateCustomerCommandValidator()
    {
        RuleFor(x => x.CustomerId).NotEmpty();
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).MaximumLength(200).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email));
        RuleFor(x => x.MobilePhone).MaximumLength(50);
        RuleFor(x => x.WorkPhone).MaximumLength(50);
        RuleFor(x => x.PersonalPhone).MaximumLength(50);
        RuleFor(x => x.IdentityNumber).MaximumLength(50);
        RuleFor(x => x.Description).MaximumLength(2000);
        RuleFor(x => x.Notes).MaximumLength(4000);
    }
}
