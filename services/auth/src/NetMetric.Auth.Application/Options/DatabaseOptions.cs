// <copyright file="DatabaseOptions.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.Auth.Application.Options;

public sealed class DatabaseOptions
{
    public const string SectionName = "Database";

    public bool ApplyMigrationsOnStartup { get; set; }
}
