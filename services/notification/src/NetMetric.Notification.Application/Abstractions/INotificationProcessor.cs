// <copyright file="INotificationProcessor.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.Notification.Domain.Entities;

namespace NetMetric.Notification.Application.Abstractions;

public interface INotificationProcessor
{
    Task ProcessAsync(
        NotificationMessage message,
        CancellationToken cancellationToken = default);
}
