// <copyright file="RunGuidedSellingQueryValidator.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentValidation;
using NetMetric.CRM.QuoteManagement.Application.Queries.Quotes;

namespace NetMetric.CRM.QuoteManagement.Application.Validators;

public sealed class RunGuidedSellingQueryValidator : AbstractValidator<RunGuidedSellingQuery>
{
    public RunGuidedSellingQueryValidator()
    {
        RuleFor(x => x.RequiredCapabilities).Must(x => x.Count <= 20);
    }
}
