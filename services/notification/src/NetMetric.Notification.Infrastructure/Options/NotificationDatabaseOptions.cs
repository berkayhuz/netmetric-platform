// <copyright file="NotificationDatabaseOptions.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.Notification.Infrastructure.Options;

public sealed class NotificationDatabaseOptions
{
    public const string SectionName = "Notification:Database";

    public bool ApplyMigrationsOnStartup { get; init; }
}
