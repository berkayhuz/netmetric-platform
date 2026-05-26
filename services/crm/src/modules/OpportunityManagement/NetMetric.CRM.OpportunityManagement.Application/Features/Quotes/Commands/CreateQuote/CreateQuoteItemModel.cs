// <copyright file="CreateQuoteItemModel.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.OpportunityManagement.Application.Features.Quotes.Commands.CreateQuote;

public sealed record CreateQuoteItemModel(Guid ProductId, string? Description, int Quantity, decimal UnitPrice, decimal DiscountRate, decimal TaxRate);
