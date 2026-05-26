// <copyright file="CreateForecastAdjustmentCommandValidator.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentValidation;
using NetMetric.CRM.SalesForecasting.Application.Commands;

namespace NetMetric.CRM.SalesForecasting.Application.Validators;

public sealed class CreateForecastAdjustmentCommandValidator : AbstractValidator<CreateForecastAdjustmentCommand>
{
    public CreateForecastAdjustmentCommandValidator()
    {
        RuleFor(x => x.PeriodEnd).GreaterThanOrEqualTo(x => x.PeriodStart);
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(256);
        RuleFor(x => x.Amount).NotEqual(0m);
    }
}
