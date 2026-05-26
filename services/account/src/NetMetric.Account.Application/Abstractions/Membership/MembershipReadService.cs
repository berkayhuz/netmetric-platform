// <copyright file="MembershipReadService.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.Account.Contracts.Organizations;

namespace NetMetric.Account.Application.Abstractions.Membership;

public interface IMembershipReadService
{
    Task<IReadOnlyCollection<OrganizationMembershipSummaryResponse>> GetMyOrganizationsAsync(
        Guid tenantId,
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<PermissionOverviewResponse> GetMyPermissionsAsync(
        Guid tenantId,
        Guid userId,
        Guid? organizationId,
        CancellationToken cancellationToken = default);
}
