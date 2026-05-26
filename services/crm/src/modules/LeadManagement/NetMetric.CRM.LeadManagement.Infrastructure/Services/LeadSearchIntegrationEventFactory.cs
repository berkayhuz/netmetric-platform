// <copyright file="LeadSearchIntegrationEventFactory.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.CRM.Sales;
using NetMetric.Search.Contracts.Documents;
using NetMetric.Search.Contracts.IntegrationEvents.V1;

namespace NetMetric.CRM.LeadManagement.Infrastructure.Services;

public static class LeadSearchIntegrationEventFactory
{
    public const string LeadReadPermission = "leads.read";
    private const string EntityType = "lead";

    public static SearchDocumentIndexRequestedV1 CreateLeadIndexRequested(
        Lead lead,
        Guid tenantId,
        string? correlationId,
        string? causationId,
        DateTimeOffset occurredAtUtc)
    {
        ArgumentNullException.ThrowIfNull(lead);

        var safeTitle = BuildSafeTitle(lead);
        ArgumentException.ThrowIfNullOrWhiteSpace(safeTitle);

        var document = new SearchDocument(
            Id: BuildDocumentId(tenantId, lead.Id),
            Source: SearchDocumentSource.Crm,
            Type: EntityType,
            Title: safeTitle,
            Summary: "Lead record.",
            Content: BuildSafeSearchContent(lead, safeTitle),
            Url: $"/leads/{lead.Id:D}",
            TenantId: tenantId,
            RequiredPermissions: [LeadReadPermission],
            Visibility: SearchDocumentVisibility.Permission,
            Locale: SearchDocumentLocales.Neutral,
            Tags: ["crm", "leads", "lead"],
            Boost: 1.0,
            CreatedAtUtc: ToUtcDateTimeOffset(lead.CreatedAt),
            UpdatedAtUtc: ToUtcDateTimeOffset(lead.UpdatedAt ?? lead.CreatedAt),
            IndexedAtUtc: DateTimeOffset.MinValue,
            IsDeleted: false,
            Metadata: BuildMetadata(lead, tenantId),
            PermissionMatchMode: SearchPermissionMatchMode.Any);

        return new SearchDocumentIndexRequestedV1(
            EventId: Guid.NewGuid(),
            Document: document,
            CorrelationId: correlationId,
            CausationId: causationId,
            OccurredAtUtc: occurredAtUtc.UtcDateTime);
    }

    public static SearchDocumentDeleteRequestedV1 CreateLeadDeleteRequested(
        Guid leadId,
        Guid tenantId,
        string? correlationId,
        string? causationId,
        DateTimeOffset occurredAtUtc)
    {
        return new SearchDocumentDeleteRequestedV1(
            EventId: Guid.NewGuid(),
            DocumentId: BuildDocumentId(tenantId, leadId),
            Source: SearchDocumentSource.Crm,
            Type: EntityType,
            TenantId: tenantId,
            CorrelationId: correlationId,
            CausationId: causationId,
            OccurredAtUtc: occurredAtUtc.UtcDateTime);
    }

    public static string BuildDocumentId(Guid tenantId, Guid leadId)
        => $"crm-lead-{tenantId:N}-{leadId:N}";

    private static string BuildSafeTitle(Lead lead)
    {
        if (!string.IsNullOrWhiteSpace(lead.FullName))
        {
            return lead.FullName.Trim();
        }

        var fullName = string.Join(' ', new[] { lead.FirstName, lead.LastName }.Where(value => !string.IsNullOrWhiteSpace(value)).Select(value => value.Trim()));
        if (!string.IsNullOrWhiteSpace(fullName))
        {
            return fullName;
        }

        if (!string.IsNullOrWhiteSpace(lead.LeadCode))
        {
            return lead.LeadCode.Trim();
        }

        throw new ArgumentException("Lead must contain at least one safe title source.");
    }

    private static string BuildSafeSearchContent(Lead lead, string safeTitle)
        => string.Join(
            '\n',
            new[]
            {
                safeTitle,
                lead.LeadCode?.Trim(),
                lead.CompanyName?.Trim()
            }.Where(value => !string.IsNullOrWhiteSpace(value)));

    private static IReadOnlyDictionary<string, string> BuildMetadata(Lead lead, Guid tenantId)
        => new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["entityId"] = lead.Id.ToString("N"),
            ["entityType"] = EntityType,
            ["tenantId"] = tenantId.ToString("N"),
            ["leadCode"] = (lead.LeadCode ?? string.Empty).Trim()
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
