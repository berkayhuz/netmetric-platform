// <copyright file="PipelineManagementOutbox.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System.Diagnostics;
using System.Text.Json;
using NetMetric.CRM.PipelineManagement.Application.Abstractions.Integration;
using NetMetric.CRM.PipelineManagement.Domain.Entities;
using NetMetric.CRM.PipelineManagement.Infrastructure.Outbox;
using NetMetric.CRM.PipelineManagement.Infrastructure.Persistence;
using NetMetric.CurrentUser;
using NetMetric.Search.Contracts.IntegrationEvents.V1;

namespace NetMetric.CRM.PipelineManagement.Infrastructure.Services;

public sealed class PipelineManagementOutbox(
    PipelineManagementDbContext dbContext,
    ICurrentUserService currentUserService) : IPipelineManagementOutbox
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    public Task EnqueuePipelineCreatedAsync(Pipeline pipeline, CancellationToken cancellationToken)
        => EnqueuePipelineSearchIndexAsync(ResolveTenantId(pipeline.TenantId), pipeline, cancellationToken);

    public Task EnqueuePipelineUpdatedAsync(Pipeline pipeline, CancellationToken cancellationToken)
        => EnqueuePipelineSearchIndexAsync(ResolveTenantId(pipeline.TenantId), pipeline, cancellationToken);

    public Task EnqueuePipelineDeletedAsync(Pipeline pipeline, CancellationToken cancellationToken)
        => EnqueuePipelineSearchDeleteAsync(ResolveTenantId(pipeline.TenantId), pipeline.Id, cancellationToken);

    private Task EnqueuePipelineSearchIndexAsync(Guid tenantId, Pipeline pipeline, CancellationToken cancellationToken)
    {
        var occurredAt = DateTimeOffset.UtcNow;
        var correlationId = GetCorrelationId();
        var idempotencyKey = $"crm:{tenantId:N}:search.pipeline.index:{pipeline.Id:N}:{occurredAt.ToUnixTimeMilliseconds()}";
        var payload = PipelineSearchIntegrationEventFactory.CreatePipelineIndexRequested(
            pipeline,
            tenantId,
            correlationId,
            correlationId,
            occurredAt);

        return AddOutboxMessageAsync(
            tenantId,
            SearchDocumentIndexRequestedV1.EventName,
            SearchDocumentIndexRequestedV1.EventVersion,
            "search.index.crm",
            payload,
            occurredAt,
            correlationId,
            idempotencyKey,
            cancellationToken);
    }

    private Task EnqueuePipelineSearchDeleteAsync(Guid tenantId, Guid pipelineId, CancellationToken cancellationToken)
    {
        var occurredAt = DateTimeOffset.UtcNow;
        var correlationId = GetCorrelationId();
        var idempotencyKey = $"crm:{tenantId:N}:search.pipeline.delete:{pipelineId:N}:{occurredAt.ToUnixTimeMilliseconds()}";
        var payload = PipelineSearchIntegrationEventFactory.CreatePipelineDeleteRequested(
            pipelineId,
            tenantId,
            correlationId,
            correlationId,
            occurredAt);

        return AddOutboxMessageAsync(
            tenantId,
            SearchDocumentDeleteRequestedV1.EventName,
            SearchDocumentDeleteRequestedV1.EventVersion,
            "search.delete.crm",
            payload,
            occurredAt,
            correlationId,
            idempotencyKey,
            cancellationToken);
    }

    private async Task AddOutboxMessageAsync<TPayload>(
        Guid tenantId,
        string eventName,
        int eventVersion,
        string routingKey,
        TPayload payload,
        DateTimeOffset occurredAt,
        string? correlationId,
        string? idempotencyKey,
        CancellationToken cancellationToken)
    {
        var message = PipelineManagementOutboxMessage.Create(
            tenantId,
            eventName,
            eventVersion,
            routingKey,
            JsonSerializer.Serialize(payload, SerializerOptions),
            occurredAt,
            correlationId,
            idempotencyKey);

        await dbContext.OutboxMessages.AddAsync(message, cancellationToken);
    }

    private static string? GetCorrelationId()
        => Activity.Current?.TraceId.ToString();

    private Guid ResolveTenantId(Guid tenantId)
        => tenantId == Guid.Empty ? currentUserService.EnsureTenant() : tenantId;
}
