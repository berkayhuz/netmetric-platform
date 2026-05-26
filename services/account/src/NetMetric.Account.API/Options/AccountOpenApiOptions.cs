// <copyright file="AccountOpenApiOptions.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.Account.Api.Options;

public sealed class AccountOpenApiOptions
{
    public const string SectionName = "OpenApi";

    public string Title { get; init; } = "NetMetric Account API";
    public string Version { get; init; } = "v1";
}
