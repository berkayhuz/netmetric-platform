// <copyright file="CreateForecastSnapshotCommandValidator.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentValidation;
using NetMetric.CRM.SalesForecasting.Application.Commands;

namespace NetMetric.CRM.SalesForecasting.Application.Validators;

public sealed class CreateForecastSnapshotCommandValidator : AbstractValidator<CreateForecastSnapshotCommand>
{
    public CreateForecastSnapshotCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(256);
        RuleFor(x => x.ForecastCategory).NotEmpty().MaximumLength(64);
        RuleFor(x => x.PeriodEnd).GreaterThanOrEqualTo(x => x.PeriodStart);
    }
}
