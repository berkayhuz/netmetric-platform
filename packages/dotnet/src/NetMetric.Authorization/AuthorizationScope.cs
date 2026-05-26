// <copyright file="AuthorizationScope.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.Authorization;

public sealed record AuthorizationScope(
    Guid TenantId,
    Guid UserId,
    string Resource,
    RowAccessLevel RowAccessLevel,
    IReadOnlyCollection<string> Permissions);
