// <copyright file="TrustedGatewayFailureReason.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.AspNetCore.TrustedGateway.Models;

public enum TrustedGatewayFailureReason
{
    None = 0,
    MissingHeaders,
    InvalidSource,
    InvalidKey,
    InvalidTimestamp,
    TimestampSkewExceeded,
    InvalidRemoteAddress,
    InvalidContentHash,
    InvalidSignature,
    ReplayDetected
}
