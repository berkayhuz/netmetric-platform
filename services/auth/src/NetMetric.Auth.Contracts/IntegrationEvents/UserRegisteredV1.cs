// <copyright file="UserRegisteredV1.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.Auth.Contracts.IntegrationEvents;

public sealed record UserRegisteredV1(
    Guid UserId,
    Guid TenantId,
    string UserName,
    string Email,
    string? FirstName,
    string? LastName,
    DateTime RegisteredAtUtc,
    string? Culture = null)
{
    public const string EventName = "auth.user.registered";
    public const int EventVersion = 1;
    public const string RoutingKey = "auth.user.registered.v1";
}
