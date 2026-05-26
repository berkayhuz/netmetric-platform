// <copyright file="AuthEmailChangeRequestedV1.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.Auth.Contracts.IntegrationEvents;

public sealed record AuthEmailChangeRequestedV1(
    Guid UserId,
    Guid TenantId,
    string UserName,
    string CurrentEmail,
    string NewEmail,
    string Token,
    string ConfirmationUrl,
    DateTime ExpiresAtUtc)
{
    public const string EventName = "auth.email.change-requested";
    public const int EventVersion = 1;
    public const string RoutingKey = "auth.email.change-requested.v1";
}
