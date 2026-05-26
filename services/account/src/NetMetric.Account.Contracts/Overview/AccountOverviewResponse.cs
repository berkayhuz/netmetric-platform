// <copyright file="AccountOverviewResponse.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.Account.Contracts.Organizations;

namespace NetMetric.Account.Contracts.Overview;

public sealed record AccountOverviewResponse(
    string DisplayName,
    string? AvatarUrl,
    bool IsMfaEnabled,
    int ActiveSessionCount,
    IReadOnlyCollection<OrganizationMembershipSummaryResponse> Organizations,
    DateTimeOffset? LastSecurityEventAt);
