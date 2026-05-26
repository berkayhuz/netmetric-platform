// <copyright file="CustomFieldOptionCommandsValidator.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentValidation;
using NetMetric.CRM.CustomerManagement.Application.Features.CustomFields.Commands.CreateCustomFieldOption;
using NetMetric.CRM.CustomerManagement.Application.Features.CustomFields.Commands.UpdateCustomFieldOption;

namespace NetMetric.CRM.CustomerManagement.Application.Validators;

public sealed class CreateCustomFieldOptionCommandValidator : AbstractValidator<CreateCustomFieldOptionCommand>
{
    public CreateCustomFieldOptionCommandValidator()
    {
        RuleFor(x => x.DefinitionId).NotEmpty();
        RuleFor(x => x.Label).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Value).NotEmpty().MaximumLength(150);
        RuleFor(x => x.OrderNo).GreaterThanOrEqualTo(0);
    }
}

public sealed class UpdateCustomFieldOptionCommandValidator : AbstractValidator<UpdateCustomFieldOptionCommand>
{
    public UpdateCustomFieldOptionCommandValidator()
    {
        RuleFor(x => x.OptionId).NotEmpty();
        RuleFor(x => x.Label).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Value).NotEmpty().MaximumLength(150);
        RuleFor(x => x.OrderNo).GreaterThanOrEqualTo(0);
    }
}
