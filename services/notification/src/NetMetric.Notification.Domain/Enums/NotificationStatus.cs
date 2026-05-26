// <copyright file="NotificationStatus.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.Notification.Domain.Enums;

public enum NotificationStatus
{
    Pending = 1,
    Queued = 2,
    Processing = 3,
    PartiallyDelivered = 4,
    Delivered = 5,
    Failed = 6,
    DeadLettered = 7
}
