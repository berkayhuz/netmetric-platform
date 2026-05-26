// <copyright file="SearchAccessContext.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.Search.Application.Security;

public sealed record SearchAccessContext(
    bool IsAuthenticated,
    Guid? TenantId,
    IReadOnlyCollection<string> Permissions)
{
    public static SearchAccessContext Anonymous { get; } = new(false, null, []);
}
