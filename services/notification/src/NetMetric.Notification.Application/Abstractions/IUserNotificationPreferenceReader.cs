// <copyright file="IUserNotificationPreferenceReader.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.Notification.Contracts.Notifications.Enums;

namespace NetMetric.Notification.Application.Abstractions;

public interface IUserNotificationPreferenceReader
{
    Task<bool> IsChannelEnabledAsync(
        Guid? tenantId,
        Guid userId,
        NotificationCategory category,
        NotificationChannel channel,
        CancellationToken cancellationToken);
}
