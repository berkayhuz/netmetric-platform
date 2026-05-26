// <copyright file="PipelineSearchIntegrationEventFactory.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.CRM.PipelineManagement.Application.Security;
using NetMetric.CRM.PipelineManagement.Domain.Entities;
using NetMetric.Search.Contracts.Documents;
using NetMetric.Search.Contracts.IntegrationEvents.V1;

namespace NetMetric.CRM.PipelineManagement.Infrastructure.Services;

public static class PipelineSearchIntegrationEventFactory
{
    private const string EntityType = "pipeline";

    public static SearchDocumentIndexRequestedV1 CreatePipelineIndexRequested(
        Pipeline pipeline,
        Guid tenantId,
        string? correlationId,
        string? causationId,
        DateTimeOffset occurredAtUtc)
    {
        ArgumentNullException.ThrowIfNull(pipeline);
        ArgumentException.ThrowIfNullOrWhiteSpace(pipeline.Name);

        var document = new SearchDocument(
            Id: BuildDocumentId(tenantId, pipeline.Id),
            Source: SearchDocumentSource.Crm,
            Type: EntityType,
            Title: pipeline.Name.Trim(),
            Summary: "Sales pipeline.",
            Content: pipeline.Name.Trim(),
            Url: $"/pipeline?pipelineId={pipeline.Id:D}",
            TenantId: tenantId,
            RequiredPermissions: [PipelineManagementPermissions.PipelinesRead],
            Visibility: SearchDocumentVisibility.Permission,
            Locale: SearchDocumentLocales.Neutral,
            Tags: ["crm", "pipelines", "pipeline"],
            Boost: 1.0,
            CreatedAtUtc: ToUtcDateTimeOffset(pipeline.CreatedAt),
            UpdatedAtUtc: ToUtcDateTimeOffset(pipeline.UpdatedAt ?? pipeline.CreatedAt),
            IndexedAtUtc: DateTimeOffset.MinValue,
            IsDeleted: false,
            Metadata: BuildMetadata(pipeline, tenantId),
            PermissionMatchMode: SearchPermissionMatchMode.Any);

        return new SearchDocumentIndexRequestedV1(
            EventId: Guid.NewGuid(),
            Document: document,
            CorrelationId: correlationId,
            CausationId: causationId,
            OccurredAtUtc: occurredAtUtc.UtcDateTime);
    }

    public static SearchDocumentDeleteRequestedV1 CreatePipelineDeleteRequested(
        Guid pipelineId,
        Guid tenantId,
        string? correlationId,
        string? causationId,
        DateTimeOffset occurredAtUtc)
    {
        return new SearchDocumentDeleteRequestedV1(
            EventId: Guid.NewGuid(),
            DocumentId: BuildDocumentId(tenantId, pipelineId),
            Source: SearchDocumentSource.Crm,
            Type: EntityType,
            TenantId: tenantId,
            CorrelationId: correlationId,
            CausationId: causationId,
            OccurredAtUtc: occurredAtUtc.UtcDateTime);
    }

    public static string BuildDocumentId(Guid tenantId, Guid pipelineId)
        => $"crm-pipeline-{tenantId:N}-{pipelineId:N}";

    private static IReadOnlyDictionary<string, string> BuildMetadata(Pipeline pipeline, Guid tenantId)
        => new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["entityId"] = pipeline.Id.ToString("N"),
            ["entityType"] = EntityType,
            ["tenantId"] = tenantId.ToString("N")
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
