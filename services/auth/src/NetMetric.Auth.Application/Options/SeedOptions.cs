// <copyright file="SeedOptions.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.Auth.Application.Options;

public sealed class SeedOptions
{
    public const string SectionName = "Seed";

    public Guid DefaultTenantId { get; set; }

    public bool AllowStartupSeed { get; set; }

    public bool AllowProductionStartupSeed { get; set; }
}
