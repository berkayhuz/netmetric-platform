// <copyright file="NotificationQueueSnapshot.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.Notification.Application.Abstractions;

public sealed record NotificationQueueSnapshot(
    long QueueDepth,
    long DeadLetterDepth,
    long ConsumerCount,
    DateTimeOffset CapturedAtUtc);
