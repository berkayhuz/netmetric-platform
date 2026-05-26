// <copyright file="QuoteCalculator.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System;
using System.Collections.Generic;
using System.Text;

namespace NetMetric.CRM.QuoteManagement.Application.Common;

public static class QuoteCalculator
{
    public static (decimal subTotal, decimal discountTotal, decimal taxTotal, decimal grandTotal) Calculate(IReadOnlyCollection<QuoteLineInput> items)
    {
        decimal subTotal = 0, discountTotal = 0, taxTotal = 0, grandTotal = 0;
        foreach (var item in items)
        {
            var lineBase = item.Quantity * item.UnitPrice;
            var lineDiscount = Math.Round(lineBase * item.DiscountRate / 100m, 2, MidpointRounding.AwayFromZero);
            var lineAfterDiscount = lineBase - lineDiscount;
            var lineTax = Math.Round(lineAfterDiscount * item.TaxRate / 100m, 2, MidpointRounding.AwayFromZero);
            subTotal += lineBase;
            discountTotal += lineDiscount;
            taxTotal += lineTax;
            grandTotal += lineAfterDiscount + lineTax;
        }

        return (Math.Round(subTotal, 2), Math.Round(discountTotal, 2), Math.Round(taxTotal, 2), Math.Round(grandTotal, 2));
    }
}
