// <copyright file="AuthDataProtectionOptions.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.Auth.Application.Options;

public sealed class AuthDataProtectionOptions
{
    public const string SectionName = "Infrastructure:DataProtection";

    public string ApplicationName { get; set; } = "NetMetric.Auth";

    public string? KeyRingPath { get; set; }

    public bool RequirePersistentKeyRingInProduction { get; set; } = true;
}
