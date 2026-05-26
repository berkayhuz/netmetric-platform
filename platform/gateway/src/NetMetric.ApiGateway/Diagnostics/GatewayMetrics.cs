// <copyright file="GatewayMetrics.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System.Diagnostics.Metrics;

namespace NetMetric.ApiGateway.Diagnostics;

public static class GatewayMetrics
{
    public static readonly Meter Meter = new("NetMetric.ApiGateway");

    public static readonly Counter<long> RateLimitRejected = Meter.CreateCounter<long>(
        "gateway.rate_limit.rejected",
        description: "Number of requests rejected by gateway rate limiting.");

    public static readonly Counter<long> TrustedGatewaySigned = Meter.CreateCounter<long>(
        "gateway.trusted_gateway.signed",
        description: "Number of upstream requests signed by the gateway.");

    public static readonly Counter<long> TrustedGatewaySigningFailed = Meter.CreateCounter<long>(
        "gateway.trusted_gateway.signing_failed",
        description: "Number of upstream requests the gateway failed to sign.");
}
