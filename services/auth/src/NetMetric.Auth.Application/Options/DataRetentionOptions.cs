// <copyright file="DataRetentionOptions.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.Auth.Application.Options;

public sealed class DataRetentionOptions
{
    public const string SectionName = "DataRetention";

    public bool EnableCleanupService { get; set; }

    public int CleanupIntervalMinutes { get; set; } = 60;

    public int AuditRetentionDays { get; set; } = 90;

    public int RevokedSessionRetentionDays { get; set; } = 30;

    public int ExpiredVerificationTokenRetentionDays { get; set; } = 7;

    public int ExpiredInvitationRetentionDays { get; set; } = 30;

    public int PublishedOutboxRetentionDays { get; set; } = 7;
}
