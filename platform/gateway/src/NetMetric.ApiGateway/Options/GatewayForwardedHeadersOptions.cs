// <copyright file="GatewayForwardedHeadersOptions.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System.ComponentModel.DataAnnotations;

namespace NetMetric.ApiGateway.Options;

public sealed class GatewayForwardedHeadersOptions
{
    public const string SectionName = "Security:ForwardedHeaders";

    [Range(1, 10)]
    public int ForwardLimit { get; set; } = 2;

    public string[] KnownProxies { get; set; } = [];

    public string[] KnownNetworks { get; set; } = [];
}
