// <copyright file="CustomerManagementOutboxProcessorOptions.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System.ComponentModel.DataAnnotations;

namespace NetMetric.CRM.CustomerManagement.Infrastructure.Outbox;

public sealed class CustomerManagementOutboxProcessorOptions
{
    public const string SectionName = "CustomerManagement:Outbox";

    public bool Enabled { get; init; } = true;

    [Range(1, 500)]
    public int BatchSize { get; init; } = 50;

    [Range(1, 300)]
    public int PollIntervalSeconds { get; init; } = 5;

    [Range(1, 120)]
    public int LeaseSeconds { get; init; } = 30;

    [Range(1, 20)]
    public int MaxRetryCount { get; init; } = 8;

    [Range(1, 3600)]
    public int MaxBackoffSeconds { get; init; } = 300;
}
