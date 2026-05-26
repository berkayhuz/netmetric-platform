// <copyright file="UpdateDealCommandValidator.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentValidation;
using NetMetric.CRM.DealManagement.Application.Commands.Deals;

namespace NetMetric.CRM.DealManagement.Application.Validators;

public sealed class UpdateDealCommandValidator : AbstractValidator<UpdateDealCommand>
{
    public UpdateDealCommandValidator()
    {
        RuleFor(x => x.DealId).NotEmpty();
        RuleFor(x => x.DealCode).NotEmpty().MaximumLength(64);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
    }
}
