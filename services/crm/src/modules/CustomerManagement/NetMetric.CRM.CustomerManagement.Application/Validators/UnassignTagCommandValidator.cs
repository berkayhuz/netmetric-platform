// <copyright file="UnassignTagCommandValidator.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentValidation;
using NetMetric.CRM.CustomerManagement.Application.Features.Tags.Commands.UnassignTag;

namespace NetMetric.CRM.CustomerManagement.Application.Validators;

public sealed class UnassignTagCommandValidator : AbstractValidator<UnassignTagCommand>
{
    public UnassignTagCommandValidator()
    {
        RuleFor(x => x.TagId).NotEmpty();
        RuleFor(x => x.EntityId).NotEmpty();
        RuleFor(x => x.EntityName).NotEmpty().MaximumLength(50);
    }
}
