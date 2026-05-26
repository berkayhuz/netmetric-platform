// <copyright file="LeadManagementOutboxPublisherTests.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentAssertions;
using Microsoft.Extensions.Options;
using NetMetric.CRM.LeadManagement.Infrastructure.Outbox;
using NetMetric.Messaging.Abstractions;
using NetMetric.Messaging.RabbitMq.Options;
using NetMetric.Search.Contracts.IntegrationEvents.V1;

namespace NetMetric.CRM.LeadManagement.UnitTests.Outbox;

public sealed class LeadManagementOutboxPublisherTests
{
    [Fact]
    public async Task PublishAsync_Should_Use_Search_Exchange_For_Search_Index_Events()
    {
        var inner = new CapturingIntegrationEventPublisher();
        var options = Options.Create(new RabbitMqOptions { Exchange = "netmetric.integration" });
        var sut = new RabbitMqLeadManagementOutboxPublisher(inner, options);
        var message = LeadManagementOutboxMessage.Create(
            Guid.NewGuid(),
            SearchDocumentIndexRequestedV1.EventName,
            SearchDocumentIndexRequestedV1.EventVersion,
            "search.index.crm",
            """{"eventId":"00000000-0000-0000-0000-000000000001"}""",
            DateTimeOffset.UtcNow,
            "corr-1",
            "idem-1");

        await sut.PublishAsync(message, CancellationToken.None);

        inner.Exchange.Should().Be("netmetric.search");
        inner.RoutingKey.Should().Be("search.index.crm");
    }

    [Fact]
    public async Task PublishAsync_Should_Use_Search_Exchange_For_Search_Delete_Events()
    {
        var inner = new CapturingIntegrationEventPublisher();
        var options = Options.Create(new RabbitMqOptions { Exchange = "netmetric.integration" });
        var sut = new RabbitMqLeadManagementOutboxPublisher(inner, options);
        var message = LeadManagementOutboxMessage.Create(
            Guid.NewGuid(),
            SearchDocumentDeleteRequestedV1.EventName,
            SearchDocumentDeleteRequestedV1.EventVersion,
            "search.delete.crm",
            """{"eventId":"00000000-0000-0000-0000-000000000001"}""",
            DateTimeOffset.UtcNow,
            "corr-1",
            "idem-1");

        await sut.PublishAsync(message, CancellationToken.None);

        inner.Exchange.Should().Be("netmetric.search");
        inner.RoutingKey.Should().Be("search.delete.crm");
    }

    private sealed class CapturingIntegrationEventPublisher : IIntegrationEventPublisher
    {
        public string? Exchange { get; private set; }
        public string? RoutingKey { get; private set; }

        public Task PublishAsync(string exchange, string routingKey, IntegrationMessage message, CancellationToken cancellationToken)
        {
            Exchange = exchange;
            RoutingKey = routingKey;
            return Task.CompletedTask;
        }
    }
}
