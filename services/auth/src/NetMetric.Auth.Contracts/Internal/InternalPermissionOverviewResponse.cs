// <copyright file="InternalPermissionOverviewResponse.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.Auth.Contracts.Internal;

public sealed record InternalPermissionOverviewResponse(
    Guid? OrganizationId,
    DateTimeOffset GeneratedAt,
    bool MayBeStale,
    IReadOnlyCollection<string> Roles,
    IReadOnlyCollection<InternalPermissionGroupResponse> PermissionGroups);
