// <copyright file="AccountMediaCleanupOptions.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.Account.Infrastructure.Media;

public sealed class AccountMediaCleanupOptions
{
    public const string SectionName = "Account:MediaCleanup";

    public bool Enabled { get; init; } = true;
    public int IntervalSeconds { get; init; } = 3600;
    public int GracePeriodMinutes { get; init; } = 5;
    public int BatchSize { get; init; } = 100;

    public TimeSpan Interval => TimeSpan.FromSeconds(IntervalSeconds);
    public TimeSpan GracePeriod => TimeSpan.FromMinutes(GracePeriodMinutes);
}
