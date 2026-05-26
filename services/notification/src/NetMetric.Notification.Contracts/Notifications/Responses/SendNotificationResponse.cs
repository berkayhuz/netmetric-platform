// <copyright file="SendNotificationResponse.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.Notification.Contracts.Notifications.Models;

namespace NetMetric.Notification.Contracts.Notifications.Responses;

public sealed record SendNotificationResponse(
    Guid NotificationId,
    bool Accepted,
    IReadOnlyCollection<NotificationChannelResult> ChannelResults);
