// <copyright file="CurrentUser.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.Tools.Domain.ValueObjects;

namespace NetMetric.Tools.Application.Abstractions.Security;

public sealed record CurrentUser(Guid UserId, string? CorrelationId, string? IpAddress, string? UserAgent);

public interface ICurrentUserAccessor
{
    CurrentUser GetRequired();
}
