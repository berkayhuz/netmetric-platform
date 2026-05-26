// <copyright file="UpsertProductRuleCommandValidator.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentValidation;
using NetMetric.CRM.QuoteManagement.Application.Commands.Quotes;

namespace NetMetric.CRM.QuoteManagement.Application.Validators;

public sealed class UpsertProductRuleCommandValidator : AbstractValidator<UpsertProductRuleCommand>
{
    public UpsertProductRuleCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(128);
        RuleFor(x => x.RuleType).NotEmpty().MaximumLength(64);
        RuleFor(x => x.Severity).NotEmpty().MaximumLength(32);
        RuleFor(x => x.Message).NotEmpty().MaximumLength(512);
        RuleFor(x => x.MinimumQuantity).GreaterThan(0).When(x => x.MinimumQuantity.HasValue);
        RuleFor(x => x.MaximumDiscountRate).InclusiveBetween(0m, 1m).When(x => x.MaximumDiscountRate.HasValue);
    }
}
