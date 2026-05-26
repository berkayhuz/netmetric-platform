// <copyright file="QuoteHandlerHelpers.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.Catalog;
using NetMetric.CRM.QuoteManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.QuoteManagement.Application.Common;
using NetMetric.CRM.QuoteManagement.Domain.Entities;
using NetMetric.CRM.Sales;
using NetMetric.CRM.Types;
using NetMetric.CurrentUser;
using NetMetric.Exceptions;

namespace NetMetric.CRM.QuoteManagement.Application.Handlers;

internal static class QuoteHandlerHelpers
{
    public static async Task<Quote> RequireQuoteAsync(IQuoteManagementDbContext dbContext, Guid quoteId, CancellationToken cancellationToken)
        => await dbContext.Quotes.Include(x => x.Items).FirstOrDefaultAsync(x => x.Id == quoteId, cancellationToken)
           ?? throw new NotFoundAppException("Quote not found.");

    public static byte[] ParseRowVersion(string? rowVersion) => string.IsNullOrWhiteSpace(rowVersion) ? [] : Convert.FromBase64String(rowVersion);

    public static async Task<IReadOnlyList<QuoteStatusHistory>> LoadHistoryAsync(IQuoteManagementDbContext dbContext, Guid quoteId, CancellationToken cancellationToken)
        => await dbContext.QuoteStatusHistories.Where(x => x.QuoteId == quoteId).OrderByDescending(x => x.ChangedAt).ToListAsync(cancellationToken);

    public static void ApplyRowVersion(Quote quote, string? rowVersion)
    {
        var expected = ParseRowVersion(rowVersion);
        if (expected.Length == 0)
            return;

        quote.RowVersion = expected;
    }

    public static void ApplyRowVersion(IQuoteManagementDbContext dbContext, Quote quote, string? rowVersion)
    {
        ApplyRowVersion(quote, rowVersion);

        if (expectedRowVersionIsMissing(rowVersion) || dbContext is not DbContext efDbContext)
            return;

        efDbContext.Entry(quote).Property(x => x.RowVersion).OriginalValue = ParseRowVersion(rowVersion);
        efDbContext.Entry(quote).Property(x => x.RowVersion).IsModified = false;
    }

    private static bool expectedRowVersionIsMissing(string? rowVersion)
        => string.IsNullOrWhiteSpace(rowVersion);

    public static async Task ValidateProductsAsync(
        IQuoteManagementDbContext dbContext,
        IReadOnlyList<QuoteLineInput> items,
        CancellationToken cancellationToken)
    {
        var requestedProductIds = items
            .Select(x => x.ProductId)
            .Where(x => x != Guid.Empty)
            .Distinct()
            .ToArray();

        if (requestedProductIds.Length == 0)
            return;

        var persistedProductIds = await dbContext.Products
            .Where(x => requestedProductIds.Contains(x.Id))
            .Select(x => x.Id)
            .ToListAsync(cancellationToken);

        var trackedProductIds = dbContext is DbContext efDbContext
            ? efDbContext.ChangeTracker.Entries()
                .Where(x => x.State != EntityState.Detached && x.State != EntityState.Deleted)
                .Select(x => x.Entity)
                .OfType<Product>()
                .Where(x => requestedProductIds.Contains(x.Id))
                .Select(x => x.Id)
                .ToList()
            : [];

        var existingProductIds = persistedProductIds
            .Concat(trackedProductIds)
            .Distinct()
            .ToArray();

        var missingProductIds = requestedProductIds
            .Except(existingProductIds)
            .Select(x => x.ToString())
            .ToArray();

        if (missingProductIds.Length == 0)
            return;

        throw new ValidationAppException(
            "One or more quote items reference unknown products.",
            new Dictionary<string, string[]>
            {
                ["items"] =
                [
                    $"Unknown product ids: {string.Join(", ", missingProductIds)}"
                ]
            });
    }

    public static void Recalculate(Quote quote, IReadOnlyList<QuoteLineInput> items, bool clearExistingItems = true)
    {
        ApplyTotals(quote, items);
        var rebuiltItems = BuildItems(quote, items);
        if (clearExistingItems)
            quote.Items.Clear();

        foreach (var item in rebuiltItems)
            quote.Items.Add(item);
    }

    public static void ApplyTotals(Quote quote, IReadOnlyList<QuoteLineInput> items)
    {
        var totals = QuoteCalculator.Calculate(items);
        quote.SubTotal = totals.subTotal;
        quote.DiscountTotal = totals.discountTotal;
        quote.TaxTotal = totals.taxTotal;
        quote.GrandTotal = totals.grandTotal;
    }

    public static List<QuoteItem> BuildItems(Quote quote, IReadOnlyList<QuoteLineInput> items)
        => items.Select(item =>
        {
            var lineBase = item.Quantity * item.UnitPrice;
            var lineDiscount = Math.Round(lineBase * item.DiscountRate / 100m, 2, MidpointRounding.AwayFromZero);
            var lineTaxBase = lineBase - lineDiscount;
            var lineTax = Math.Round(lineTaxBase * item.TaxRate / 100m, 2, MidpointRounding.AwayFromZero);

            return new QuoteItem
            {
                TenantId = quote.TenantId,
                QuoteId = quote.Id,
                ProductId = item.ProductId,
                Description = string.IsNullOrWhiteSpace(item.Description) ? null : item.Description.Trim(),
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                DiscountRate = item.DiscountRate,
                TaxRate = item.TaxRate,
                DiscountAmount = lineDiscount,
                TaxAmount = lineTax,
                Total = lineTaxBase,
                LineTotal = lineTaxBase + lineTax
            };
        }).ToList();

    public static async Task AddHistoryAsync(IQuoteManagementDbContext dbContext, ICurrentUserService currentUserService, Quote quote, QuoteStatusType? oldStatus, QuoteStatusType newStatus, string? note, CancellationToken cancellationToken)
    {
        await dbContext.QuoteStatusHistories.AddAsync(new QuoteStatusHistory
        {
            TenantId = currentUserService.TenantId,
            QuoteId = quote.Id,
            OldStatus = oldStatus,
            NewStatus = newStatus,
            ChangedAt = DateTime.UtcNow,
            ChangedByUserId = currentUserService.UserId,
            Note = string.IsNullOrWhiteSpace(note) ? null : note.Trim()
        }, cancellationToken);
    }
}
