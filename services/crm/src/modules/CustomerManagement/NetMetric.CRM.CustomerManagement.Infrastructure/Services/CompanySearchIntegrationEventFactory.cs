// <copyright file="CompanySearchIntegrationEventFactory.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.CRM.Core;
using NetMetric.Search.Contracts.Documents;
using NetMetric.Search.Contracts.IntegrationEvents.V1;

namespace NetMetric.CRM.CustomerManagement.Infrastructure.Services;

public static class CompanySearchIntegrationEventFactory
{
    public const string CompanyReadPermission = "crm.customer-management.companies.read";
    private const string EntityType = "company";

    public static SearchDocumentIndexRequestedV1 CreateCompanyIndexRequested(
        Company company,
        Guid tenantId,
        string? correlationId,
        string? causationId,
        DateTimeOffset occurredAtUtc)
    {
        ArgumentNullException.ThrowIfNull(company);
        ArgumentException.ThrowIfNullOrWhiteSpace(company.Name);

        var document = new SearchDocument(
            Id: BuildDocumentId(tenantId, company.Id),
            Source: SearchDocumentSource.Crm,
            Type: EntityType,
            Title: company.Name.Trim(),
            Summary: "Company record.",
            Content: BuildSafeSearchContent(company),
            Url: $"/companies/{company.Id:D}",
            TenantId: tenantId,
            RequiredPermissions: [CompanyReadPermission],
            Visibility: SearchDocumentVisibility.Permission,
            Locale: SearchDocumentLocales.Neutral,
            Tags: ["crm", "companies", "company"],
            Boost: 1.0,
            CreatedAtUtc: ToUtcDateTimeOffset(company.CreatedAt),
            UpdatedAtUtc: ToUtcDateTimeOffset(company.UpdatedAt ?? company.CreatedAt),
            IndexedAtUtc: DateTimeOffset.MinValue,
            IsDeleted: false,
            Metadata: BuildMetadata(company, tenantId),
            PermissionMatchMode: SearchPermissionMatchMode.Any);

        return new SearchDocumentIndexRequestedV1(
            EventId: Guid.NewGuid(),
            Document: document,
            CorrelationId: correlationId,
            CausationId: causationId,
            OccurredAtUtc: occurredAtUtc.UtcDateTime);
    }

    public static SearchDocumentDeleteRequestedV1 CreateCompanyDeleteRequested(
        Guid companyId,
        Guid tenantId,
        string? correlationId,
        string? causationId,
        DateTimeOffset occurredAtUtc)
    {
        return new SearchDocumentDeleteRequestedV1(
            EventId: Guid.NewGuid(),
            DocumentId: BuildDocumentId(tenantId, companyId),
            Source: SearchDocumentSource.Crm,
            Type: EntityType,
            TenantId: tenantId,
            CorrelationId: correlationId,
            CausationId: causationId,
            OccurredAtUtc: occurredAtUtc.UtcDateTime);
    }

    public static string BuildDocumentId(Guid tenantId, Guid companyId)
        => $"crm-company-{tenantId:N}-{companyId:N}";

    private static IReadOnlyDictionary<string, string> BuildMetadata(Company company, Guid tenantId)
        => new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["entityId"] = company.Id.ToString("N"),
            ["entityType"] = EntityType,
            ["tenantId"] = tenantId.ToString("N")
        };

    private static string BuildSafeSearchContent(Company company)
        => string.Join(
            '\n',
            new[]
            {
                company.Name.Trim(),
                TrimToNull(company.Website)
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
