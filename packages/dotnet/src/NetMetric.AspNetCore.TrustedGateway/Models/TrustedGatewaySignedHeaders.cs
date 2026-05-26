// <copyright file="TrustedGatewaySignedHeaders.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.AspNetCore.TrustedGateway.Models;

public sealed record TrustedGatewaySignedHeaders(
    string Signature,
    string Timestamp,
    string KeyId,
    string Source,
    string Nonce,
    string ContentHash,
    string CorrelationId);
