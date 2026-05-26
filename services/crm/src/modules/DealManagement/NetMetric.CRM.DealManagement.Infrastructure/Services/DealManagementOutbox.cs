// <copyright file="DealManagementOutbox.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System.Diagnostics;
using System.Text.Json;
using NetMetric.CRM.DealManagement.Application.Abstractions.Integration;
using NetMetric.CRM.DealManagement.Application.IntegrationEvents;
using NetMetric.CRM.DealManagement.Infrastructure.Outbox;
using NetMetric.CRM.DealManagement.Infrastructure.Persistence;
using NetMetric.CRM.Sales;
using NetMetric.CurrentUser;
using NetMetric.Search.Contracts.IntegrationEvents.V1;

namespace NetMetric.CRM.DealManagement.Infrastructure.Services;

public sealed class DealManagementOutbox(
    DealManagementDbContext dbContext,
    ICurrentUserService currentUserService) : IDealManagementOutbox
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    public Task EnqueueDealCreatedAsync(Deal deal, CancellationToken cancellationToken)
        => EnqueueDealSearchIndexAsync(ResolveTenantId(deal.TenantId), deal, cancellationToken);

    public Task EnqueueDealUpdatedAsync(Deal deal, CancellationToken cancellationToken)
        => EnqueueDealSearchIndexAsync(ResolveTenantId(deal.TenantId), deal, cancellationToken);

    public Task EnqueueDealDeletedAsync(Deal deal, CancellationToken cancellationToken)
        => EnqueueDealSearchDeleteAsync(ResolveTenantId(deal.TenantId), deal.Id, cancellationToken);

    public Task EnqueueDealRestoredAsync(Deal deal, CancellationToken cancellationToken)
        => EnqueueDealSearchIndexAsync(ResolveTenantId(deal.TenantId), deal, cancellationToken);

    public Task EnqueueDealPurgedAsync(
        Guid tenantId,
        Guid dealId,
        string? dealName,
        Guid? ownerUserId,
        CancellationToken cancellationToken)
        => EnqueueLifecycleAsync(
            tenantId,
            dealId,
            DealManagementIntegrationEventNames.DealPurged,
            ownerUserId,
            new Dictionary<string, string>
            {
                ["dealName"] = string.IsNullOrWhiteSpace(dealName) ? "Deleted deal" : dealName.Trim()
            },
            cancellationToken);

    public async Task EnqueueDealCreatedAndPersistAsync(Deal deal, CancellationToken cancellationToken)
    {
        await EnqueueDealCreatedAsync(deal, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private Task EnqueueDealSearchIndexAsync(Guid tenantId, Deal deal, CancellationToken cancellationToken)
    {
        var occurredAt = DateTimeOffset.UtcNow;
        var correlationId = GetCorrelationId();
        var idempotencyKey = $"crm:{tenantId:N}:search.deal.index:{deal.Id:N}:{occurredAt.ToUnixTimeMilliseconds()}";
        var payload = DealSearchIntegrationEventFactory.CreateDealIndexRequested(
            deal,
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

    private Task EnqueueDealSearchDeleteAsync(Guid tenantId, Guid dealId, CancellationToken cancellationToken)
    {
        var occurredAt = DateTimeOffset.UtcNow;
        var correlationId = GetCorrelationId();
        var idempotencyKey = $"crm:{tenantId:N}:search.deal.delete:{dealId:N}:{occurredAt.ToUnixTimeMilliseconds()}";
        var payload = DealSearchIntegrationEventFactory.CreateDealDeleteRequested(
            dealId,
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
        var message = DealManagementOutboxMessage.Create(
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

    private Task EnqueueLifecycleAsync(
        Guid tenantId,
        Guid entityId,
        string eventType,
        Guid? ownerUserId,
        IReadOnlyDictionary<string, string> metadata,
        CancellationToken cancellationToken)
    {
        var occurredAt = DateTimeOffset.UtcNow;
        var correlationId = GetCorrelationId();
        var idempotencyKey = $"crm:{tenantId:N}:{eventType}:{entityId:N}:lifecycle";
        var payload = new DealLifecycleIntegrationEventV1(
            Guid.NewGuid(),
            tenantId,
            entityId,
            "deal",
            eventType,
            ownerUserId,
            metadata,
            correlationId,
            occurredAt);

        return AddOutboxMessageAsync(
            tenantId,
            eventType,
            DealLifecycleIntegrationEventV1.EventVersion,
            "crm.deal-management.lifecycle",
            payload,
            occurredAt,
            correlationId,
            idempotencyKey,
            cancellationToken);
    }

    private static string? GetCorrelationId()
        => Activity.Current?.TraceId.ToString();

    private Guid ResolveTenantId(Guid tenantId)
        => tenantId == Guid.Empty ? currentUserService.EnsureTenant() : tenantId;
}

