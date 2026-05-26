// <copyright file="QuoteCrossWriterService.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.CRM.QuoteManagement.Application.Abstractions.Integration;
using NetMetric.CRM.QuoteManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.QuoteManagement.Application.Abstractions.Services;
using NetMetric.CRM.QuoteManagement.Application.Common;
using NetMetric.CRM.QuoteManagement.Application.Handlers;
using NetMetric.CRM.QuoteManagement.Contracts.Integration;
using NetMetric.CRM.Sales;
using NetMetric.CRM.Types;
using NetMetric.CurrentUser;

namespace NetMetric.CRM.QuoteManagement.Application.Services;

public sealed class QuoteCrossWriterService(
    IQuoteManagementDbContext dbContext,
    IQuoteProductReadModelSyncService quoteProductReadModelSyncService,
    IQuoteOpportunityReadModelSyncService quoteOpportunityReadModelSyncService,
    IQuoteCustomerReadModelSyncService quoteCustomerReadModelSyncService,
    ICurrentUserService currentUserService,
    IQuoteManagementOutbox outbox) : IQuoteCrossWriterService
{
    public async Task<QuoteCrossWriterCreateResult> CreateAsync(QuoteCrossWriterCreateRequest request, CancellationToken cancellationToken = default)
    {
        currentUserService.EnsureAuthenticated();
        var tenantId = currentUserService.EnsureTenant();
        var actor = currentUserService.UserName;
        var utcNow = DateTime.UtcNow;
        var opportunity = await quoteOpportunityReadModelSyncService.SyncAsync(
            request.OpportunityId,
            cancellationToken);
        var resolvedCustomerId = request.CustomerId ?? opportunity.CustomerId;
        if (resolvedCustomerId.HasValue)
            await quoteCustomerReadModelSyncService.SyncAsync(resolvedCustomerId.Value, cancellationToken);

        var items = request.Items
            .Select(x => new QuoteLineInput(x.ProductId, x.Description, x.Quantity, x.UnitPrice, x.DiscountRate, x.TaxRate))
            .ToArray();

        var quote = new Quote
        {
            TenantId = tenantId,
            QuoteNumber = request.QuoteNumber.Trim(),
            QuoteDate = request.QuoteDate,
            ValidUntil = request.ValidUntil,
            OpportunityId = request.OpportunityId,
            CustomerId = resolvedCustomerId,
            OwnerUserId = request.OwnerUserId,
            CurrencyCode = request.CurrencyCode.Trim().ToUpperInvariant(),
            ExchangeRate = request.ExchangeRate,
            TermsAndConditions = string.IsNullOrWhiteSpace(request.TermsAndConditions) ? null : request.TermsAndConditions.Trim(),
            Status = QuoteStatusType.Draft,
            RevisionNumber = 1,
            CreatedAt = utcNow,
            UpdatedAt = utcNow,
            CreatedBy = actor,
            UpdatedBy = actor
        };

        await quoteProductReadModelSyncService.SyncAsync(items.Select(x => x.ProductId).ToArray(), cancellationToken);
        await QuoteHandlerHelpers.ValidateProductsAsync(dbContext, items, cancellationToken);
        QuoteHandlerHelpers.Recalculate(quote, items);

        await dbContext.Quotes.AddAsync(quote, cancellationToken);
        await QuoteHandlerHelpers.AddHistoryAsync(dbContext, currentUserService, quote, null, QuoteStatusType.Draft, "Quote created from opportunity.", cancellationToken);
        await outbox.EnqueueQuoteCreatedAsync(quote, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return ToResult(quote);
    }

    private static QuoteCrossWriterCreateResult ToResult(Quote quote)
        => new(
            quote.Id,
            quote.QuoteNumber,
            quote.OpportunityId,
            quote.QuoteDate,
            quote.ValidUntil,
            quote.SubTotal,
            quote.DiscountTotal,
            quote.TaxTotal,
            quote.GrandTotal,
            quote.TermsAndConditions,
            quote.OwnerUserId,
            quote.CurrencyCode,
            quote.ExchangeRate,
            quote.Items.Select(ToResult).ToList(),
            Convert.ToBase64String(quote.RowVersion));

    private static QuoteCrossWriterItemResult ToResult(QuoteItem item)
        => new(
            item.Id,
            item.ProductId ?? Guid.Empty,
            item.Description,
            item.Quantity,
            item.UnitPrice,
            item.DiscountRate,
            item.TaxRate,
            item.LineTotal);
}
