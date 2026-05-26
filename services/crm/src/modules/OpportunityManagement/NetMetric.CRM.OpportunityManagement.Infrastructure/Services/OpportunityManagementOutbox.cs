// <copyright file="OpportunityManagementOutbox.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System.Diagnostics;
using System.Text.Json;
using NetMetric.CRM.OpportunityManagement.Application.Abstractions.Integration;
using NetMetric.CRM.OpportunityManagement.Application.IntegrationEvents;
using NetMetric.CRM.OpportunityManagement.Infrastructure.Outbox;
using NetMetric.CRM.OpportunityManagement.Infrastructure.Persistence;
using NetMetric.CRM.Sales;
using NetMetric.CurrentUser;
using NetMetric.Search.Contracts.IntegrationEvents.V1;

namespace NetMetric.CRM.OpportunityManagement.Infrastructure.Services;

public sealed class OpportunityManagementOutbox(
    OpportunityManagementDbContext dbContext,
    ICurrentUserService currentUserService) : IOpportunityManagementOutbox
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    public Task EnqueueOpportunityCreatedAsync(Opportunity opportunity, CancellationToken cancellationToken)
        => EnqueueOpportunitySearchIndexAsync(ResolveTenantId(opportunity.TenantId), opportunity, cancellationToken);

    public Task EnqueueOpportunityUpdatedAsync(Opportunity opportunity, CancellationToken cancellationToken)
        => EnqueueOpportunitySearchIndexAsync(ResolveTenantId(opportunity.TenantId), opportunity, cancellationToken);

    public Task EnqueueOpportunityDeletedAsync(Opportunity opportunity, CancellationToken cancellationToken)
        => EnqueueOpportunitySearchDeleteAsync(ResolveTenantId(opportunity.TenantId), opportunity.Id, cancellationToken);

    public Task EnqueueOpportunityRestoredAsync(Opportunity opportunity, CancellationToken cancellationToken)
        => EnqueueOpportunitySearchIndexAsync(ResolveTenantId(opportunity.TenantId), opportunity, cancellationToken);

    public Task EnqueueOpportunityPurgedAsync(
        Guid tenantId,
        Guid opportunityId,
        string? opportunityName,
        Guid? ownerUserId,
        CancellationToken cancellationToken)
        => EnqueueLifecycleAsync(
            tenantId,
            opportunityId,
            OpportunityManagementIntegrationEventNames.OpportunityPurged,
            ownerUserId,
            new Dictionary<string, string>
            {
                ["opportunityName"] = string.IsNullOrWhiteSpace(opportunityName) ? "Deleted opportunity" : opportunityName.Trim()
            },
            cancellationToken);

    public async Task EnqueueOpportunityCreatedAndPersistAsync(Opportunity opportunity, CancellationToken cancellationToken)
    {
        await EnqueueOpportunityCreatedAsync(opportunity, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private Task EnqueueOpportunitySearchIndexAsync(Guid tenantId, Opportunity opportunity, CancellationToken cancellationToken)
    {
        var occurredAt = DateTimeOffset.UtcNow;
        var correlationId = GetCorrelationId();
        var idempotencyKey = $"crm:{tenantId:N}:search.opportunity.index:{opportunity.Id:N}:{occurredAt.ToUnixTimeMilliseconds()}";
        var payload = OpportunitySearchIntegrationEventFactory.CreateOpportunityIndexRequested(opportunity, tenantId, correlationId, correlationId, occurredAt);

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

    private Task EnqueueOpportunitySearchDeleteAsync(Guid tenantId, Guid opportunityId, CancellationToken cancellationToken)
    {
        var occurredAt = DateTimeOffset.UtcNow;
        var correlationId = GetCorrelationId();
        var idempotencyKey = $"crm:{tenantId:N}:search.opportunity.delete:{opportunityId:N}:{occurredAt.ToUnixTimeMilliseconds()}";
        var payload = OpportunitySearchIntegrationEventFactory.CreateOpportunityDeleteRequested(opportunityId, tenantId, correlationId, correlationId, occurredAt);

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
        var message = OpportunityManagementOutboxMessage.Create(
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
        var payload = new OpportunityLifecycleIntegrationEventV1(
            Guid.NewGuid(),
            tenantId,
            entityId,
            "opportunity",
            eventType,
            ownerUserId,
            metadata,
            correlationId,
            occurredAt);

        return AddOutboxMessageAsync(
            tenantId,
            eventType,
            OpportunityLifecycleIntegrationEventV1.EventVersion,
            "crm.opportunity-management.lifecycle",
            payload,
            occurredAt,
            correlationId,
            idempotencyKey,
            cancellationToken);
    }

    private static string? GetCorrelationId() => Activity.Current?.TraceId.ToString();
    private Guid ResolveTenantId(Guid tenantId) => tenantId == Guid.Empty ? currentUserService.EnsureTenant() : tenantId;
}
