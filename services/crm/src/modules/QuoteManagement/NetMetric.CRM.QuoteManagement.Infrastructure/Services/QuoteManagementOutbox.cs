// <copyright file="QuoteManagementOutbox.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System.Diagnostics;
using System.Text.Json;
using NetMetric.CRM.QuoteManagement.Application.Abstractions.Integration;
using NetMetric.CRM.QuoteManagement.Application.IntegrationEvents;
using NetMetric.CRM.QuoteManagement.Infrastructure.Outbox;
using NetMetric.CRM.QuoteManagement.Infrastructure.Persistence;
using NetMetric.CRM.Sales;
using NetMetric.CurrentUser;
using NetMetric.Search.Contracts.IntegrationEvents.V1;

namespace NetMetric.CRM.QuoteManagement.Infrastructure.Services;

public sealed class QuoteManagementOutbox(
    QuoteManagementDbContext dbContext,
    ICurrentUserService currentUserService) : IQuoteManagementOutbox
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    public Task EnqueueQuoteCreatedAsync(Quote quote, CancellationToken cancellationToken)
        => EnqueueQuoteSearchIndexAsync(ResolveTenantId(quote.TenantId), quote, cancellationToken);

    public Task EnqueueQuoteUpdatedAsync(Quote quote, CancellationToken cancellationToken)
        => EnqueueQuoteSearchIndexAsync(ResolveTenantId(quote.TenantId), quote, cancellationToken);

    public Task EnqueueQuoteDeletedAsync(Quote quote, CancellationToken cancellationToken)
        => EnqueueQuoteSearchDeleteAsync(ResolveTenantId(quote.TenantId), quote.Id, cancellationToken);

    public Task EnqueueQuoteRestoredAsync(Quote quote, CancellationToken cancellationToken)
        => EnqueueQuoteSearchIndexAsync(ResolveTenantId(quote.TenantId), quote, cancellationToken);

    public Task EnqueueQuotePurgedAsync(
        Guid tenantId,
        Guid quoteId,
        string? quoteNumber,
        Guid? ownerUserId,
        CancellationToken cancellationToken)
        => EnqueueLifecycleAsync(
            tenantId,
            quoteId,
            QuoteManagementIntegrationEventNames.QuotePurged,
            ownerUserId,
            new Dictionary<string, string>
            {
                ["quoteNumber"] = string.IsNullOrWhiteSpace(quoteNumber) ? "Deleted quote" : quoteNumber.Trim()
            },
            cancellationToken);

    public async Task EnqueueQuoteCreatedAndPersistAsync(Quote quote, CancellationToken cancellationToken)
    {
        await EnqueueQuoteCreatedAsync(quote, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private Task EnqueueQuoteSearchIndexAsync(Guid tenantId, Quote quote, CancellationToken cancellationToken)
    {
        var occurredAt = DateTimeOffset.UtcNow;
        var correlationId = GetCorrelationId();
        var idempotencyKey = $"crm:{tenantId:N}:search.quote.index:{quote.Id:N}:{occurredAt.ToUnixTimeMilliseconds()}";
        var payload = QuoteSearchIntegrationEventFactory.CreateQuoteIndexRequested(
            quote,
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

    private Task EnqueueQuoteSearchDeleteAsync(Guid tenantId, Guid quoteId, CancellationToken cancellationToken)
    {
        var occurredAt = DateTimeOffset.UtcNow;
        var correlationId = GetCorrelationId();
        var idempotencyKey = $"crm:{tenantId:N}:search.quote.delete:{quoteId:N}:{occurredAt.ToUnixTimeMilliseconds()}";
        var payload = QuoteSearchIntegrationEventFactory.CreateQuoteDeleteRequested(
            quoteId,
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
        var message = QuoteManagementOutboxMessage.Create(
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
        var payload = new QuoteLifecycleIntegrationEventV1(
            Guid.NewGuid(),
            tenantId,
            entityId,
            "quote",
            eventType,
            ownerUserId,
            metadata,
            correlationId,
            occurredAt);

        return AddOutboxMessageAsync(
            tenantId,
            eventType,
            QuoteLifecycleIntegrationEventV1.EventVersion,
            "crm.quote-management.lifecycle",
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
