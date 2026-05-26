// <copyright file="InternalIdentityOptions.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.Auth.API.Security;

public sealed class InternalIdentityOptions
{
    public const string SectionName = "Security:InternalIdentity";

    public string[] AllowedSources { get; init; } = ["NetMetric.Account"];
}
