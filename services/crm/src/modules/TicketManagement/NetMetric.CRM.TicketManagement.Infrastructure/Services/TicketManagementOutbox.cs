// <copyright file="TicketManagementOutbox.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System.Diagnostics;
using System.Text.Json;
using NetMetric.CRM.Support;
using NetMetric.CRM.TicketManagement.Application.Abstractions.Integration;
using NetMetric.CRM.TicketManagement.Application.IntegrationEvents;
using NetMetric.CRM.TicketManagement.Infrastructure.Outbox;
using NetMetric.CRM.TicketManagement.Infrastructure.Persistence;
using NetMetric.CurrentUser;
using NetMetric.Search.Contracts.IntegrationEvents.V1;

namespace NetMetric.CRM.TicketManagement.Infrastructure.Services;

public sealed class TicketManagementOutbox(
    TicketManagementDbContext dbContext,
    ICurrentUserService currentUserService) : ITicketManagementOutbox
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    public Task EnqueueTicketCreatedAsync(Ticket ticket, CancellationToken cancellationToken)
        => EnqueueTicketSearchIndexAsync(ResolveTenantId(ticket.TenantId), ticket, cancellationToken);

    public Task EnqueueTicketUpdatedAsync(Ticket ticket, CancellationToken cancellationToken)
        => EnqueueTicketSearchIndexAsync(ResolveTenantId(ticket.TenantId), ticket, cancellationToken);

    public Task EnqueueTicketDeletedAsync(Ticket ticket, CancellationToken cancellationToken)
        => EnqueueTicketSearchDeleteAsync(ResolveTenantId(ticket.TenantId), ticket.Id, cancellationToken);

    public Task EnqueueTicketRestoredAsync(Ticket ticket, CancellationToken cancellationToken)
        => EnqueueTicketSearchIndexAsync(ResolveTenantId(ticket.TenantId), ticket, cancellationToken);

    public Task EnqueueTicketPurgedAsync(
        Guid tenantId,
        Guid ticketId,
        string? subject,
        Guid? assigneeUserId,
        CancellationToken cancellationToken)
        => EnqueueLifecycleAsync(
            tenantId,
            ticketId,
            TicketManagementIntegrationEventNames.TicketPurged,
            assigneeUserId,
            new Dictionary<string, string>
            {
                ["subject"] = string.IsNullOrWhiteSpace(subject) ? "Deleted ticket" : subject.Trim()
            },
            cancellationToken);

    private Task EnqueueTicketSearchIndexAsync(Guid tenantId, Ticket ticket, CancellationToken cancellationToken)
    {
        var occurredAt = DateTimeOffset.UtcNow;
        var correlationId = GetCorrelationId();
        var idempotencyKey = $"crm:{tenantId:N}:search.ticket.index:{ticket.Id:N}:{occurredAt.ToUnixTimeMilliseconds()}";
        var payload = TicketSearchIntegrationEventFactory.CreateTicketIndexRequested(
            ticket,
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

    private Task EnqueueTicketSearchDeleteAsync(Guid tenantId, Guid ticketId, CancellationToken cancellationToken)
    {
        var occurredAt = DateTimeOffset.UtcNow;
        var correlationId = GetCorrelationId();
        var idempotencyKey = $"crm:{tenantId:N}:search.ticket.delete:{ticketId:N}:{occurredAt.ToUnixTimeMilliseconds()}";
        var payload = TicketSearchIntegrationEventFactory.CreateTicketDeleteRequested(
            ticketId,
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
        var message = TicketManagementOutboxMessage.Create(
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
        var payload = new TicketLifecycleIntegrationEventV1(
            Guid.NewGuid(),
            tenantId,
            entityId,
            "ticket",
            eventType,
            ownerUserId,
            metadata,
            correlationId,
            occurredAt);

        return AddOutboxMessageAsync(
            tenantId,
            eventType,
            TicketLifecycleIntegrationEventV1.EventVersion,
            "crm.ticket-management.lifecycle",
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
