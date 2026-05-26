// <copyright file="AccountDatabaseOptions.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.Account.Persistence.Options;

public sealed class AccountDatabaseOptions
{
    public const string SectionName = "Database";

    public bool ApplyMigrationsOnStartup { get; init; }
    public int CommandTimeoutSeconds { get; init; } = 30;
    public int MaxRetryCount { get; init; } = 5;
    public int MaxRetryDelaySeconds { get; init; } = 10;
}
