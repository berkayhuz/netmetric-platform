// <copyright file="NotificationDispatchOptions.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System.ComponentModel.DataAnnotations;

namespace NetMetric.Notification.Application.Options;

public sealed class NotificationDispatchOptions
{
    public const string SectionName = "Notification:Dispatch";

    [Range(1, 10)]
    public int MaxAttempts { get; init; } = 3;

    [Range(0, 300)]
    public int RetryDelayMilliseconds { get; init; } = 250;

    [Range(0, 86_400)]
    public int MaxRetryDelaySeconds { get; init; } = 30;
}
