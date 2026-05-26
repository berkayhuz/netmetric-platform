// <copyright file="ITrustedGatewayRequestValidator.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.AspNetCore.Http;
using NetMetric.AspNetCore.TrustedGateway.Models;

namespace NetMetric.AspNetCore.TrustedGateway.Abstractions;

public interface ITrustedGatewayRequestValidator
{
    Task<TrustedGatewayValidationResult> ValidateAsync(HttpContext context, string correlationId, CancellationToken cancellationToken);
}
