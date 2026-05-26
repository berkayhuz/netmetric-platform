// <copyright file="AccountIntegrationEventPublisher.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NetMetric.Messaging.Abstractions;
using NetMetric.Messaging.RabbitMq.Options;
using NetMetric.Notification.Contracts.IntegrationEvents.V1;

namespace NetMetric.Account.Infrastructure.IntegrationEvents;

public interface IAccountIntegrationEventPublisher
{
    Task PublishAsync(string type, string payloadJson, string? correlationId, CancellationToken cancellationToken = default);
}

public sealed class LoggingAccountIntegrationEventPublisher(ILogger<LoggingAccountIntegrationEventPublisher> logger)
    : IAccountIntegrationEventPublisher
{
    public Task PublishAsync(string type, string payloadJson, string? correlationId, CancellationToken cancellationToken = default)
    {
        logger.LogInformation(
            "Account integration event queued for broker publishing. Type={EventType} CorrelationId={CorrelationId}",
            type,
            correlationId);

        return Task.CompletedTask;
    }
}

public sealed class RabbitMqAccountIntegrationEventPublisher(
    IIntegrationEventPublisher publisher,
    IOptions<RabbitMqOptions> rabbitMqOptions,
    ILogger<RabbitMqAccountIntegrationEventPublisher> logger)
    : IAccountIntegrationEventPublisher
{
    public async Task PublishAsync(string type, string payloadJson, string? correlationId, CancellationToken cancellationToken = default)
    {
        var eventId = Guid.NewGuid();
        var eventName = MapEventName(type);
        await publisher.PublishAsync(
            rabbitMqOptions.Value.Exchange,
            MapRoutingKey(type),
            new IntegrationMessage(
                new IntegrationEventMetadata(
                    eventId,
                    eventName,
                    1,
                    "NetMetric.Account",
                    DateTime.UtcNow,
                    correlationId,
                    null),
                payloadJson),
            cancellationToken);

        logger.LogInformation(
            "Published Account integration event. EventId={EventId} Type={EventType} EventName={EventName} CorrelationId={CorrelationId}",
            eventId,
            type,
            eventName,
            correlationId);
    }

    private static string MapEventName(string type) =>
        type switch
        {
            Outbox.OutboxEventTypes.SecurityNotificationRequested => SecurityNotificationRequestedV1.EventName,
            Outbox.OutboxEventTypes.SecurityEventRaised => "account.security_event.raised",
            Outbox.OutboxEventTypes.ProfileUpdated => "account.profile.updated",
            Outbox.OutboxEventTypes.PreferencesUpdated => "account.preferences.updated",
            Outbox.OutboxEventTypes.AvatarChanged => "account.avatar.changed",
            Outbox.OutboxEventTypes.AvatarDeleted => "account.avatar.deleted",
            Outbox.OutboxEventTypes.SessionRevoked => "account.session.revoked",
            _ => type
        };

    private static string MapRoutingKey(string type) =>
        type switch
        {
            Outbox.OutboxEventTypes.SecurityNotificationRequested => SecurityNotificationRequestedV1.RoutingKey,
            Outbox.OutboxEventTypes.SecurityEventRaised => "account.security_event.raised.v1",
            Outbox.OutboxEventTypes.ProfileUpdated => "account.profile.updated.v1",
            Outbox.OutboxEventTypes.PreferencesUpdated => "account.preferences.updated.v1",
            Outbox.OutboxEventTypes.AvatarChanged => "account.avatar.changed.v1",
            Outbox.OutboxEventTypes.AvatarDeleted => "account.avatar.deleted.v1",
            Outbox.OutboxEventTypes.SessionRevoked => "account.session.revoked.v1",
            _ => type
        };
}
