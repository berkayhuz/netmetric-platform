// <copyright file="INotificationQueue.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.Notification.Domain.Entities;

namespace NetMetric.Notification.Application.Abstractions;

public interface INotificationQueue
{
    Task EnqueueAsync(
        NotificationMessage message,
        CancellationToken cancellationToken = default);

    Task<NotificationMessage?> DequeueAsync(CancellationToken cancellationToken);

    Task CompleteAsync(
        NotificationMessage message,
        CancellationToken cancellationToken = default);

    Task AbandonAsync(
        NotificationMessage message,
        bool requeue,
        CancellationToken cancellationToken = default);
}
