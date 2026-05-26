// <copyright file="TrustedGatewayValidationResult.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.AspNetCore.TrustedGateway.Models;

public sealed record TrustedGatewayValidationResult(bool Succeeded, TrustedGatewayFailureReason FailureReason, string? ErrorCode = null)
{
    public static TrustedGatewayValidationResult Success() => new(true, TrustedGatewayFailureReason.None);

    public static TrustedGatewayValidationResult Fail(TrustedGatewayFailureReason reason, string code) => new(false, reason, code);
}
