// <copyright file="AllowAllNotificationPreferenceReader.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.Notification.Application.Abstractions;
using NetMetric.Notification.Contracts.Notifications.Enums;

namespace NetMetric.Notification.Application.Services;

public sealed class AllowAllNotificationPreferenceReader : IUserNotificationPreferenceReader
{
    public Task<bool> IsChannelEnabledAsync(
        Guid? tenantId,
        Guid userId,
        NotificationCategory category,
        NotificationChannel channel,
        CancellationToken cancellationToken) =>
        Task.FromResult(true);
}
