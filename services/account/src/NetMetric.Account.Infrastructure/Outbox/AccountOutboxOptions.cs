// <copyright file="AccountOutboxOptions.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.Account.Infrastructure.Outbox;

public sealed class AccountOutboxOptions
{
    public const string SectionName = "Outbox";

    public bool EnableProcessor { get; init; } = true;
    public int PollingIntervalSeconds { get; init; } = 5;
    public int BatchSize { get; init; } = 50;
    public int MaxAttempts { get; init; } = 12;
    public int PoisonDelayMinutes { get; init; } = 60;
}
