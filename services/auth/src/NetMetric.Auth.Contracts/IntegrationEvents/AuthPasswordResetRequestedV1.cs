// <copyright file="AuthPasswordResetRequestedV1.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.Auth.Contracts.IntegrationEvents;

public sealed record AuthPasswordResetRequestedV1(
    Guid UserId,
    Guid TenantId,
    string UserName,
    string Email,
    string Token,
    string ResetUrl,
    DateTime ExpiresAtUtc)
{
    public const string EventName = "auth.password.reset-requested";
    public const int EventVersion = 1;
    public const string RoutingKey = "auth.password.reset-requested.v1";
}
