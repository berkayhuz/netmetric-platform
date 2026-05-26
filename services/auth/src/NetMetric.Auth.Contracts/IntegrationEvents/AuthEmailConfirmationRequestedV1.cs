// <copyright file="AuthEmailConfirmationRequestedV1.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.Auth.Contracts.IntegrationEvents;

public sealed record AuthEmailConfirmationRequestedV1(
    Guid UserId,
    Guid TenantId,
    string UserName,
    string Email,
    string Token,
    string ConfirmationUrl,
    DateTime ExpiresAtUtc,
    string? Culture = null)
{
    public const string EventName = "auth.email.confirmation-requested";
    public const int EventVersion = 1;
    public const string RoutingKey = "auth.email.confirmation-requested.v1";
}
