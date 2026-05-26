// <copyright file="DealSearchIntegrationEventFactory.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.CRM.Sales;
using NetMetric.Search.Contracts.Documents;
using NetMetric.Search.Contracts.IntegrationEvents.V1;

namespace NetMetric.CRM.DealManagement.Infrastructure.Services;

public static class DealSearchIntegrationEventFactory
{
    public const string DealReadPermission = "deals.read";
    private const string EntityType = "deal";

    public static SearchDocumentIndexRequestedV1 CreateDealIndexRequested(
        Deal deal,
        Guid tenantId,
        string? correlationId,
        string? causationId,
        DateTimeOffset occurredAtUtc)
    {
        ArgumentNullException.ThrowIfNull(deal);
        ArgumentException.ThrowIfNullOrWhiteSpace(deal.Name);
        ArgumentException.ThrowIfNullOrWhiteSpace(deal.DealCode);

        var title = deal.Name.Trim();
        var safeCode = deal.DealCode.Trim();

        var document = new SearchDocument(
            Id: BuildDocumentId(tenantId, deal.Id),
            Source: SearchDocumentSource.Crm,
            Type: EntityType,
            Title: title,
            Summary: safeCode,
            Content: string.Join('\n', new[] { title, safeCode }),
            Url: $"/deals/{deal.Id:D}",
            TenantId: tenantId,
            RequiredPermissions: [DealReadPermission],
            Visibility: SearchDocumentVisibility.Permission,
            Locale: SearchDocumentLocales.Neutral,
            Tags: ["crm", "deals", "deal"],
            Boost: 1.0,
            CreatedAtUtc: ToUtcDateTimeOffset(deal.CreatedAt),
            UpdatedAtUtc: ToUtcDateTimeOffset(deal.UpdatedAt ?? deal.CreatedAt),
            IndexedAtUtc: DateTimeOffset.MinValue,
            IsDeleted: false,
            Metadata: BuildMetadata(deal, tenantId, safeCode),
            PermissionMatchMode: SearchPermissionMatchMode.Any);

        return new SearchDocumentIndexRequestedV1(
            EventId: Guid.NewGuid(),
            Document: document,
            CorrelationId: correlationId,
            CausationId: causationId,
            OccurredAtUtc: occurredAtUtc.UtcDateTime);
    }

    public static SearchDocumentDeleteRequestedV1 CreateDealDeleteRequested(
        Guid dealId,
        Guid tenantId,
        string? correlationId,
        string? causationId,
        DateTimeOffset occurredAtUtc)
    {
        return new SearchDocumentDeleteRequestedV1(
            EventId: Guid.NewGuid(),
            DocumentId: BuildDocumentId(tenantId, dealId),
            Source: SearchDocumentSource.Crm,
            Type: EntityType,
            TenantId: tenantId,
            CorrelationId: correlationId,
            CausationId: causationId,
            OccurredAtUtc: occurredAtUtc.UtcDateTime);
    }

    public static string BuildDocumentId(Guid tenantId, Guid dealId)
        => $"crm-deal-{tenantId:N}-{dealId:N}";

    private static IReadOnlyDictionary<string, string> BuildMetadata(Deal deal, Guid tenantId, string safeCode)
        => new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["entityId"] = deal.Id.ToString("N"),
            ["entityType"] = EntityType,
            ["tenantId"] = tenantId.ToString("N"),
            ["dealCode"] = safeCode
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
