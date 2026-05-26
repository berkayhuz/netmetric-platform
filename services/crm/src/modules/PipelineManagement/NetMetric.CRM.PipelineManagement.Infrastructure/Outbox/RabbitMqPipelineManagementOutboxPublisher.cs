// <copyright file="RabbitMqPipelineManagementOutboxPublisher.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.Extensions.Options;
using NetMetric.Messaging.Abstractions;
using NetMetric.Messaging.RabbitMq.Options;
using NetMetric.Search.Contracts.IntegrationEvents.V1;

namespace NetMetric.CRM.PipelineManagement.Infrastructure.Outbox;

public sealed class RabbitMqPipelineManagementOutboxPublisher(
    IIntegrationEventPublisher publisher,
    IOptions<RabbitMqOptions> rabbitMqOptions) : IPipelineManagementOutboxPublisher
{
    private const string SearchExchange = "netmetric.search";

    public Task PublishAsync(PipelineManagementOutboxMessage message, CancellationToken cancellationToken)
    {
        using var _ = System.Text.Json.JsonDocument.Parse(message.PayloadJson);

        var integrationMessage = new IntegrationMessage(
            new IntegrationEventMetadata(
                message.Id,
                message.EventName,
                message.EventVersion,
                "crm.pipeline-management",
                message.OccurredAtUtc.UtcDateTime,
                message.CorrelationId,
                message.CorrelationId),
            message.PayloadJson);

        var exchange = IsSearchEvent(message.EventName) ? SearchExchange : rabbitMqOptions.Value.Exchange;

        return publisher.PublishAsync(
            exchange,
            message.RoutingKey,
            integrationMessage,
            cancellationToken);
    }

    private static bool IsSearchEvent(string eventName)
        => eventName.Equals(SearchDocumentIndexRequestedV1.EventName, StringComparison.Ordinal) ||
           eventName.Equals(SearchDocumentDeleteRequestedV1.EventName, StringComparison.Ordinal) ||
           eventName.Equals(SearchReindexRequestedV1.EventName, StringComparison.Ordinal);
}
