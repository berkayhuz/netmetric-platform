// <copyright file="NotificationChannelResult.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.Notification.Contracts.Notifications.Enums;

namespace NetMetric.Notification.Contracts.Notifications.Models;

public sealed record NotificationChannelResult(
    NotificationChannel Channel,
    bool Succeeded,
    string? Provider,
    string? ExternalMessageId,
    string? ErrorCode,
    string? ErrorMessage,
    int AttemptCount);
