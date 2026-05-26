// <copyright file="CreateQuoteCommandValidator.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentValidation;
using NetMetric.CRM.QuoteManagement.Application.Commands.Quotes;

namespace NetMetric.CRM.QuoteManagement.Application.Validators;

public sealed class UpdateQuoteCommandValidator : AbstractValidator<UpdateQuoteCommand>
{
    public UpdateQuoteCommandValidator()
    {
        AddCommonRules();

        RuleFor(x => x.QuoteId).NotEmpty();
        RuleFor(x => x.RowVersion).NotEmpty();
    }

    private void AddCommonRules()
    {
        RuleFor(x => x.QuoteNumber).NotEmpty().MaximumLength(64);
        RuleFor(x => x.CurrencyCode).NotEmpty().Length(3);
        RuleFor(x => x.ExchangeRate).GreaterThan(0);
        RuleFor(x => x.Items).NotEmpty();

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(x => x.ProductId).NotEmpty();
            item.RuleFor(x => x.Quantity).GreaterThan(0);
            item.RuleFor(x => x.UnitPrice).GreaterThanOrEqualTo(0);
            item.RuleFor(x => x.DiscountRate).InclusiveBetween(0, 100);
            item.RuleFor(x => x.TaxRate).InclusiveBetween(0, 100);
        });
    }
}
