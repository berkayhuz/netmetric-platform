// <copyright file="TicketCategoryCommandsValidator.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentValidation;
using NetMetric.CRM.TicketManagement.Application.Commands.Categories;

namespace NetMetric.CRM.TicketManagement.Application.Validators;

public sealed class TicketCategoryCommandsValidator :
    AbstractValidator<CreateTicketCategoryCommand>
{
    public TicketCategoryCommandsValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Description).MaximumLength(500);
    }
}
