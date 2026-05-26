// <copyright file="CustomFieldDefinitionCommandsValidator.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentValidation;
using NetMetric.CRM.CustomerManagement.Application.Features.CustomFields.Commands.CreateCustomFieldDefinition;
using NetMetric.CRM.CustomerManagement.Application.Features.CustomFields.Commands.UpdateCustomFieldDefinition;

namespace NetMetric.CRM.CustomerManagement.Application.Validators;

public sealed class CreateCustomFieldDefinitionCommandValidator : AbstractValidator<CreateCustomFieldDefinitionCommand>
{
    public CreateCustomFieldDefinitionCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Label).NotEmpty().MaximumLength(150);
        RuleFor(x => x.EntityName).NotEmpty().MaximumLength(50);
        RuleFor(x => x.OrderNo).GreaterThanOrEqualTo(0);
    }
}

public sealed class UpdateCustomFieldDefinitionCommandValidator : AbstractValidator<UpdateCustomFieldDefinitionCommand>
{
    public UpdateCustomFieldDefinitionCommandValidator()
    {
        RuleFor(x => x.DefinitionId).NotEmpty();
        RuleFor(x => x.Label).NotEmpty().MaximumLength(150);
        RuleFor(x => x.OrderNo).GreaterThanOrEqualTo(0);
    }
}
