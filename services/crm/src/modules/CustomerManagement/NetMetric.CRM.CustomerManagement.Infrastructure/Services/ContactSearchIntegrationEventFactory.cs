// <copyright file="ContactSearchIntegrationEventFactory.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.CRM.Core;
using NetMetric.Search.Contracts.Documents;
using NetMetric.Search.Contracts.IntegrationEvents.V1;

namespace NetMetric.CRM.CustomerManagement.Infrastructure.Services;

public static class ContactSearchIntegrationEventFactory
{
    public const string ContactReadPermission = "crm.customer-management.contacts.read";
    private const string EntityType = "contact";

    public static SearchDocumentIndexRequestedV1 CreateContactIndexRequested(
        Contact contact,
        Guid tenantId,
        string? correlationId,
        string? causationId,
        DateTimeOffset occurredAtUtc)
    {
        ArgumentNullException.ThrowIfNull(contact);
        ArgumentException.ThrowIfNullOrWhiteSpace(contact.FullName);

        var document = new SearchDocument(
            Id: BuildDocumentId(tenantId, contact.Id),
            Source: SearchDocumentSource.Crm,
            Type: EntityType,
            Title: contact.FullName.Trim(),
            Summary: "Contact record.",
            Content: BuildSafeSearchContent(contact),
            Url: $"/contacts/{contact.Id:D}",
            TenantId: tenantId,
            RequiredPermissions: [ContactReadPermission],
            Visibility: SearchDocumentVisibility.Permission,
            Locale: SearchDocumentLocales.Neutral,
            Tags: ["crm", "contacts", "contact"],
            Boost: 1.0,
            CreatedAtUtc: ToUtcDateTimeOffset(contact.CreatedAt),
            UpdatedAtUtc: ToUtcDateTimeOffset(contact.UpdatedAt ?? contact.CreatedAt),
            IndexedAtUtc: DateTimeOffset.MinValue,
            IsDeleted: false,
            Metadata: BuildMetadata(contact, tenantId),
            PermissionMatchMode: SearchPermissionMatchMode.Any);

        return new SearchDocumentIndexRequestedV1(
            EventId: Guid.NewGuid(),
            Document: document,
            CorrelationId: correlationId,
            CausationId: causationId,
            OccurredAtUtc: occurredAtUtc.UtcDateTime);
    }

    public static SearchDocumentDeleteRequestedV1 CreateContactDeleteRequested(
        Guid contactId,
        Guid tenantId,
        string? correlationId,
        string? causationId,
        DateTimeOffset occurredAtUtc)
    {
        return new SearchDocumentDeleteRequestedV1(
            EventId: Guid.NewGuid(),
            DocumentId: BuildDocumentId(tenantId, contactId),
            Source: SearchDocumentSource.Crm,
            Type: EntityType,
            TenantId: tenantId,
            CorrelationId: correlationId,
            CausationId: causationId,
            OccurredAtUtc: occurredAtUtc.UtcDateTime);
    }

    public static string BuildDocumentId(Guid tenantId, Guid contactId)
        => $"crm-contact-{tenantId:N}-{contactId:N}";

    private static IReadOnlyDictionary<string, string> BuildMetadata(Contact contact, Guid tenantId)
    {
        var metadata = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["entityId"] = contact.Id.ToString("N"),
            ["entityType"] = EntityType,
            ["tenantId"] = tenantId.ToString("N")
        };

        if (contact.CompanyId.HasValue)
        {
            metadata["companyId"] = contact.CompanyId.Value.ToString("N");
        }

        return metadata;
    }

    private static string BuildSafeSearchContent(Contact contact)
        => string.Join(
            '\n',
            new[]
            {
                contact.FullName.Trim(),
                TrimToNull(contact.Title),
                TrimToNull(contact.JobTitle),
                TrimToNull(contact.Company?.Name)
            }.Where(value => !string.IsNullOrWhiteSpace(value)));

    private static string? TrimToNull(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

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
