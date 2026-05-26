// <copyright file="NotificationDeliveryStatus.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.Notification.Domain.Enums;

public enum NotificationDeliveryStatus
{
    Pending = 1,
    Succeeded = 2,
    Failed = 3,
    Skipped = 4
}
