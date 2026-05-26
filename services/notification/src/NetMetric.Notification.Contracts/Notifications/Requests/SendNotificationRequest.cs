// <copyright file="SendNotificationRequest.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.Notification.Contracts.Notifications.Enums;
using NetMetric.Notification.Contracts.Notifications.Models;

namespace NetMetric.Notification.Contracts.Notifications.Requests;

public sealed record SendNotificationRequest(
    Guid? TenantId,
    NotificationRecipient Recipient,
    IReadOnlyCollection<NotificationChannel> Channels,
    NotificationCategory Category,
    NotificationPriority Priority,
    string Subject,
    string TextBody,
    string? HtmlBody,
    string? TemplateKey,
    IReadOnlyDictionary<string, string> Metadata,
    string? CorrelationId,
    string? IdempotencyKey);
