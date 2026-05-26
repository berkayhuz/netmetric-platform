// <copyright file="UpsertProductBundleCommandValidator.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentValidation;
using NetMetric.CRM.QuoteManagement.Application.Commands.Quotes;

namespace NetMetric.CRM.QuoteManagement.Application.Validators;

public sealed class UpsertProductBundleCommandValidator : AbstractValidator<UpsertProductBundleCommand>
{
    public UpsertProductBundleCommandValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(64);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(128);
        RuleFor(x => x.DiscountRate).InclusiveBetween(0m, 1m);
        RuleFor(x => x.Items).NotEmpty();
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(x => x.ProductId).NotEmpty();
            item.RuleFor(x => x.Quantity).GreaterThan(0);
        });
    }
}
