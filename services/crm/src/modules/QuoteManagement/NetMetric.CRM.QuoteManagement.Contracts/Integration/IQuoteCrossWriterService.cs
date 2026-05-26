// <copyright file="IQuoteCrossWriterService.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.QuoteManagement.Contracts.Integration;

public interface IQuoteCrossWriterService
{
    Task<QuoteCrossWriterCreateResult> CreateAsync(QuoteCrossWriterCreateRequest request, CancellationToken cancellationToken = default);
}

public sealed record QuoteCrossWriterCreateRequest(
    Guid OpportunityId,
    Guid? CustomerId,
    string QuoteNumber,
    DateTime QuoteDate,
    DateTime? ValidUntil,
    string? TermsAndConditions,
    Guid? OwnerUserId,
    string CurrencyCode,
    decimal ExchangeRate,
    IReadOnlyList<QuoteCrossWriterItemRequest> Items);

public sealed record QuoteCrossWriterItemRequest(
    Guid ProductId,
    string? Description,
    int Quantity,
    decimal UnitPrice,
    decimal DiscountRate,
    decimal TaxRate);

public sealed record QuoteCrossWriterCreateResult(
    Guid Id,
    string QuoteNumber,
    Guid? OpportunityId,
    DateTime QuoteDate,
    DateTime? ValidUntil,
    decimal? SubTotal,
    decimal? DiscountTotal,
    decimal? TaxTotal,
    decimal? GrandTotal,
    string? TermsAndConditions,
    Guid? OwnerUserId,
    string CurrencyCode,
    decimal? ExchangeRate,
    IReadOnlyList<QuoteCrossWriterItemResult> Items,
    string RowVersion);

public sealed record QuoteCrossWriterItemResult(
    Guid Id,
    Guid ProductId,
    string? Description,
    int Quantity,
    decimal UnitPrice,
    decimal DiscountRate,
    decimal TaxRate,
    decimal LineTotal);
