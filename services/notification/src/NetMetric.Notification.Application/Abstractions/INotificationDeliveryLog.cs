// <copyright file="INotificationDeliveryLog.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.Notification.Contracts.Notifications.Enums;
using NetMetric.Notification.Contracts.Notifications.Models;
using NetMetric.Notification.Contracts.Notifications.Requests;
using NetMetric.Notification.Domain.Entities;

namespace NetMetric.Notification.Application.Abstractions;

public interface INotificationDeliveryLog
{
    Task<NotificationMessage> CreateNotificationAsync(
        SendNotificationRequest request,
        CancellationToken cancellationToken);

    Task<NotificationMessage?> GetNotificationAsync(
        Guid notificationId,
        CancellationToken cancellationToken);

    Task<IReadOnlySet<NotificationChannel>> GetSucceededChannelsAsync(
        Guid notificationId,
        CancellationToken cancellationToken);

    Task MarkQueuedAsync(
        Guid notificationId,
        CancellationToken cancellationToken);

    Task MarkProcessingAsync(
        Guid notificationId,
        CancellationToken cancellationToken);

    Task MarkDeadLetteredAsync(
        Guid notificationId,
        string reason,
        CancellationToken cancellationToken);

    Task RecordAttemptAsync(
        Guid notificationId,
        NotificationChannelResult result,
        string? recipient,
        CancellationToken cancellationToken);
}
