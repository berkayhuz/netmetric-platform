// <copyright file="QuoteDetailDto.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.OpportunityManagement.Contracts.DTOs;

public sealed record QuoteDetailDto(Guid Id, string QuoteNumber, Guid? OpportunityId, DateTime QuoteDate, DateTime? ValidUntil, decimal? SubTotal, decimal? DiscountTotal, decimal? TaxTotal, decimal? GrandTotal, string? TermsAndConditions, Guid? OwnerUserId, string CurrencyCode, decimal? ExchangeRate, IReadOnlyList<QuoteItemDto> Items, string RowVersion);
