// <copyright file="IntegrationJobProcessingOptions.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.IntegrationHub.Infrastructure.Processing;

public sealed class IntegrationJobProcessingOptions
{
    public bool Enabled { get; set; }
    public int BatchSize { get; set; } = 10;
    public int MaxAttempts { get; set; } = 3;
    public int PollIntervalSeconds { get; set; } = 30;
    public int LeaseSeconds { get; set; } = 300;
    public int BaseRetryDelaySeconds { get; set; } = 30;
    public int MaxRetryDelaySeconds { get; set; } = 900;

    public TimeSpan PollInterval => TimeSpan.FromSeconds(Math.Max(5, PollIntervalSeconds));
    public TimeSpan LeaseDuration => TimeSpan.FromSeconds(Math.Max(30, LeaseSeconds));
    public TimeSpan BaseRetryDelay => TimeSpan.FromSeconds(Math.Max(1, BaseRetryDelaySeconds));
    public TimeSpan MaxRetryDelay => TimeSpan.FromSeconds(Math.Max(BaseRetryDelaySeconds, MaxRetryDelaySeconds));
}
