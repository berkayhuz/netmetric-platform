// <copyright file="LeadManagementOutbox.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System.Diagnostics;
using System.Text.Json;
using NetMetric.CRM.LeadManagement.Application.Abstractions.Integration;
using NetMetric.CRM.LeadManagement.Application.IntegrationEvents;
using NetMetric.CRM.LeadManagement.Infrastructure.Outbox;
using NetMetric.CRM.LeadManagement.Infrastructure.Persistence;
using NetMetric.CRM.Sales;
using NetMetric.CurrentUser;
using NetMetric.Search.Contracts.IntegrationEvents.V1;

namespace NetMetric.CRM.LeadManagement.Infrastructure.Services;

public sealed class LeadManagementOutbox(
    LeadManagementDbContext dbContext,
    ICurrentUserService currentUserService) : ILeadManagementOutbox
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    public Task EnqueueLeadCreatedAsync(Lead lead, CancellationToken cancellationToken)
        => EnqueueLeadSearchIndexAsync(ResolveTenantId(lead.TenantId), lead, cancellationToken);

    public Task EnqueueLeadUpdatedAsync(Lead lead, CancellationToken cancellationToken)
        => EnqueueLeadSearchIndexAsync(ResolveTenantId(lead.TenantId), lead, cancellationToken);

    public Task EnqueueLeadDeletedAsync(Lead lead, CancellationToken cancellationToken)
        => EnqueueLeadSearchDeleteAsync(ResolveTenantId(lead.TenantId), lead.Id, cancellationToken);

    public Task EnqueueLeadRestoredAsync(Lead lead, CancellationToken cancellationToken)
        => EnqueueLeadSearchIndexAsync(ResolveTenantId(lead.TenantId), lead, cancellationToken);

    public Task EnqueueLeadPurgedAsync(
        Guid tenantId,
        Guid leadId,
        string? leadName,
        Guid? ownerUserId,
        CancellationToken cancellationToken)
        => EnqueueLifecycleAsync(
            tenantId,
            leadId,
            LeadManagementIntegrationEventNames.LeadPurged,
            ownerUserId,
            new Dictionary<string, string>
            {
                ["leadName"] = string.IsNullOrWhiteSpace(leadName) ? "Deleted lead" : leadName.Trim()
            },
            cancellationToken);

    private Task EnqueueLeadSearchIndexAsync(Guid tenantId, Lead lead, CancellationToken cancellationToken)
    {
        var occurredAt = DateTimeOffset.UtcNow;
        var correlationId = GetCorrelationId();
        var idempotencyKey = $"crm:{tenantId:N}:search.lead.index:{lead.Id:N}:{occurredAt.ToUnixTimeMilliseconds()}";
        var payload = LeadSearchIntegrationEventFactory.CreateLeadIndexRequested(
            lead,
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

    private Task EnqueueLeadSearchDeleteAsync(Guid tenantId, Guid leadId, CancellationToken cancellationToken)
    {
        var occurredAt = DateTimeOffset.UtcNow;
        var correlationId = GetCorrelationId();
        var idempotencyKey = $"crm:{tenantId:N}:search.lead.delete:{leadId:N}:{occurredAt.ToUnixTimeMilliseconds()}";
        var payload = LeadSearchIntegrationEventFactory.CreateLeadDeleteRequested(
            leadId,
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
        var message = LeadManagementOutboxMessage.Create(
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
        var payload = new LeadLifecycleIntegrationEventV1(
            Guid.NewGuid(),
            tenantId,
            entityId,
            "lead",
            eventType,
            ownerUserId,
            metadata,
            correlationId,
            occurredAt);

        return AddOutboxMessageAsync(
            tenantId,
            eventType,
            LeadLifecycleIntegrationEventV1.EventVersion,
            "crm.lead-management.lifecycle",
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
