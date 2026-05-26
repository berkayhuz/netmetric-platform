// <copyright file="InternalOrganizationMembershipSummaryResponse.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.Auth.Contracts.Internal;

public sealed record InternalOrganizationMembershipSummaryResponse(
    Guid OrganizationId,
    Guid TenantId,
    string OrganizationName,
    string? OrganizationSlug,
    string Status,
    bool IsDefault,
    DateTimeOffset JoinedAt,
    DateTimeOffset? LastPermissionRefreshAt,
    IReadOnlyCollection<string> Roles);
