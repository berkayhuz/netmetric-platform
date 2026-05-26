// <copyright file="CustomerManagementTrashRetentionOptions.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System.ComponentModel.DataAnnotations;

namespace NetMetric.CRM.CustomerManagement.Infrastructure.TrashRetention;

public sealed class CustomerManagementTrashRetentionOptions
{
    public const string SectionName = "CustomerManagement:TrashRetention";

    public bool Enabled { get; init; } = false;

    [Range(30, 86400)]
    public int IntervalSeconds { get; init; } = 300;

    [Range(1, 500)]
    public int BatchSize { get; init; } = 100;

    [Range(1, 5000)]
    public int MaxTenantsPerRun { get; init; } = 250;

    [Range(0, 3600)]
    public int InitialDelaySeconds { get; init; } = 30;
}
