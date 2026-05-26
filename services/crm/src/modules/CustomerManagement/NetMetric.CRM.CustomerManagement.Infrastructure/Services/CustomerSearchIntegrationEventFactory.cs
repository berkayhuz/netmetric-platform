// <copyright file="CustomerSearchIntegrationEventFactory.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.CRM.Core;
using NetMetric.Search.Contracts.Documents;
using NetMetric.Search.Contracts.IntegrationEvents.V1;

namespace NetMetric.CRM.CustomerManagement.Infrastructure.Services;

public static class CustomerSearchIntegrationEventFactory
{
    public const string CustomerReadPermission = "crm.customer-management.customers.read";
    private const string EntityType = "customer";

    public static SearchDocumentIndexRequestedV1 CreateCustomerIndexRequested(
        Customer customer,
        Guid tenantId,
        string? correlationId,
        string? causationId,
        DateTimeOffset occurredAtUtc)
    {
        ArgumentNullException.ThrowIfNull(customer);
        ArgumentException.ThrowIfNullOrWhiteSpace(customer.FullName);

        var document = new SearchDocument(
            Id: BuildDocumentId(tenantId, customer.Id),
            Source: SearchDocumentSource.Crm,
            Type: EntityType,
            Title: customer.FullName.Trim(),
            Summary: $"{customer.CustomerType} customer record.",
            Content: BuildSafeSearchContent(customer),
            Url: $"/customers/{customer.Id:D}",
            TenantId: tenantId,
            RequiredPermissions: [CustomerReadPermission],
            Visibility: SearchDocumentVisibility.Permission,
            Locale: SearchDocumentLocales.Neutral,
            Tags: ["crm", "customers", "customer"],
            Boost: 1.0,
            CreatedAtUtc: ToUtcDateTimeOffset(customer.CreatedAt),
            UpdatedAtUtc: ToUtcDateTimeOffset(customer.UpdatedAt ?? customer.CreatedAt),
            IndexedAtUtc: DateTimeOffset.MinValue,
            IsDeleted: false,
            Metadata: BuildMetadata(customer, tenantId),
            PermissionMatchMode: SearchPermissionMatchMode.Any);

        return new SearchDocumentIndexRequestedV1(
            EventId: Guid.NewGuid(),
            Document: document,
            CorrelationId: correlationId,
            CausationId: causationId,
            OccurredAtUtc: occurredAtUtc.UtcDateTime);
    }

    public static SearchDocumentDeleteRequestedV1 CreateCustomerDeleteRequested(
        Guid customerId,
        Guid tenantId,
        string? correlationId,
        string? causationId,
        DateTimeOffset occurredAtUtc)
    {
        return new SearchDocumentDeleteRequestedV1(
            EventId: Guid.NewGuid(),
            DocumentId: BuildDocumentId(tenantId, customerId),
            Source: SearchDocumentSource.Crm,
            Type: EntityType,
            TenantId: tenantId,
            CorrelationId: correlationId,
            CausationId: causationId,
            OccurredAtUtc: occurredAtUtc.UtcDateTime);
    }

    public static string BuildDocumentId(Guid tenantId, Guid customerId)
        => $"crm-customer-{tenantId:N}-{customerId:N}";

    private static IReadOnlyDictionary<string, string> BuildMetadata(Customer customer, Guid tenantId)
    {
        var metadata = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["entityId"] = customer.Id.ToString("N"),
            ["entityType"] = EntityType,
            ["tenantId"] = tenantId.ToString("N")
        };

        if (customer.CompanyId.HasValue)
        {
            metadata["companyId"] = customer.CompanyId.Value.ToString("N");
        }

        return metadata;
    }

    private static string BuildSafeSearchContent(Customer customer)
        => $"{customer.FullName.Trim()}\n{customer.CustomerType} customer";

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
