// <copyright file="NotificationRequestedV1.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.Notification.Contracts.Notifications.Enums;
using NetMetric.Notification.Contracts.Notifications.Models;

namespace NetMetric.Notification.Contracts.IntegrationEvents.V1;

public sealed record NotificationRequestedV1(
    Guid EventId,
    Guid? TenantId,
    Guid? UserId,
    string Source,
    NotificationCategory Category,
    NotificationPriority Priority,
    NotificationRecipient Recipient,
    IReadOnlyCollection<NotificationChannel> Channels,
    string Subject,
    string TextBody,
    string? HtmlBody,
    NotificationTemplateData Template,
    IReadOnlyDictionary<string, string> Metadata,
    string? CorrelationId,
    string? IdempotencyKey,
    DateTime OccurredAtUtc)
{
    public const string EventName = "notification.requested";
    public const int EventVersion = 1;
    public const string RoutingKey = "notification.requested.v1";
}
