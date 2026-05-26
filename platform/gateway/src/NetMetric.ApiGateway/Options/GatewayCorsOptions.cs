// <copyright file="GatewayCorsOptions.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System.ComponentModel.DataAnnotations;

namespace NetMetric.ApiGateway.Options;

public sealed class GatewayCorsOptions
{
    public const string SectionName = "Security:Cors";
    public const string PolicyName = "gateway-cors";

    [MinLength(1)]
    public string[] AllowedOrigins { get; set; } = [];

    public bool AllowCredentials { get; set; } = true;
}
