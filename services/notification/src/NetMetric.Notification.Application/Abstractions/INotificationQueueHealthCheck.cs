// <copyright file="INotificationQueueHealthCheck.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.Notification.Application.Abstractions;

public interface INotificationQueueHealthCheck
{
    Task<bool> CanConnectAsync(CancellationToken cancellationToken = default);

    Task<NotificationQueueSnapshot?> GetSnapshotAsync(CancellationToken cancellationToken = default);
}
