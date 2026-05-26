// <copyright file="CurrentUser.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.Account.Application.Abstractions.Security;

public sealed record CurrentUser(
    Guid TenantId,
    Guid UserId,
    Guid? SessionId,
    DateTimeOffset? AuthenticatedAt,
    IReadOnlyCollection<string> AuthenticationMethods,
    string? CorrelationId,
    string? IpAddress,
    string? UserAgent);

public interface ICurrentUserAccessor
{
    CurrentUser GetRequired();
}
