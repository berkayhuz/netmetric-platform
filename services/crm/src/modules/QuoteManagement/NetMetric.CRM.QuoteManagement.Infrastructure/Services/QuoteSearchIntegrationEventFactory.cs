// <copyright file="QuoteSearchIntegrationEventFactory.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.CRM.Sales;
using NetMetric.Search.Contracts.Documents;
using NetMetric.Search.Contracts.IntegrationEvents.V1;

namespace NetMetric.CRM.QuoteManagement.Infrastructure.Services;

public static class QuoteSearchIntegrationEventFactory
{
    public const string QuoteReadPermission = "quotes.read";
    private const string EntityType = "quote";

    public static SearchDocumentIndexRequestedV1 CreateQuoteIndexRequested(
        Quote quote,
        Guid tenantId,
        string? correlationId,
        string? causationId,
        DateTimeOffset occurredAtUtc)
    {
        ArgumentNullException.ThrowIfNull(quote);
        ArgumentException.ThrowIfNullOrWhiteSpace(quote.QuoteNumber);

        var safeQuoteNumber = quote.QuoteNumber.Trim();

        var document = new SearchDocument(
            Id: BuildDocumentId(tenantId, quote.Id),
            Source: SearchDocumentSource.Crm,
            Type: EntityType,
            Title: safeQuoteNumber,
            Summary: safeQuoteNumber,
            Content: safeQuoteNumber,
            Url: $"/quotes/{quote.Id:D}",
            TenantId: tenantId,
            RequiredPermissions: [QuoteReadPermission],
            Visibility: SearchDocumentVisibility.Permission,
            Locale: SearchDocumentLocales.Neutral,
            Tags: ["crm", "quotes", "quote"],
            Boost: 1.0,
            CreatedAtUtc: ToUtcDateTimeOffset(quote.CreatedAt),
            UpdatedAtUtc: ToUtcDateTimeOffset(quote.UpdatedAt ?? quote.CreatedAt),
            IndexedAtUtc: DateTimeOffset.MinValue,
            IsDeleted: false,
            Metadata: BuildMetadata(quote, tenantId, safeQuoteNumber),
            PermissionMatchMode: SearchPermissionMatchMode.Any);

        return new SearchDocumentIndexRequestedV1(
            EventId: Guid.NewGuid(),
            Document: document,
            CorrelationId: correlationId,
            CausationId: causationId,
            OccurredAtUtc: occurredAtUtc.UtcDateTime);
    }

    public static SearchDocumentDeleteRequestedV1 CreateQuoteDeleteRequested(
        Guid quoteId,
        Guid tenantId,
        string? correlationId,
        string? causationId,
        DateTimeOffset occurredAtUtc)
        => new(
            EventId: Guid.NewGuid(),
            DocumentId: BuildDocumentId(tenantId, quoteId),
            Source: SearchDocumentSource.Crm,
            Type: EntityType,
            TenantId: tenantId,
            CorrelationId: correlationId,
            CausationId: causationId,
            OccurredAtUtc: occurredAtUtc.UtcDateTime);

    public static string BuildDocumentId(Guid tenantId, Guid quoteId)
        => $"crm-quote-{tenantId:N}-{quoteId:N}";

    private static IReadOnlyDictionary<string, string> BuildMetadata(Quote quote, Guid tenantId, string safeQuoteNumber)
        => new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["entityId"] = quote.Id.ToString("N"),
            ["entityType"] = EntityType,
            ["tenantId"] = tenantId.ToString("N"),
            ["quoteNumber"] = safeQuoteNumber
        };

    private static DateTimeOffset ToUtcDateTimeOffset(DateTime value)
    {
        var utcValue = value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
        };

        return new DateTimeOffset(utcValue);
    }
}
