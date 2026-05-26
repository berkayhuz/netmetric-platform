// <copyright file="AccountManagementContracts.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.Account.Contracts.Management;

public sealed record CreateAccountInvitationRequest(
    string Email,
    string? FirstName,
    string? LastName);

public sealed record AccountInvitationResponse(
    Guid TenantId,
    Guid InvitationId,
    string Email,
    DateTime ExpiresAtUtc,
    string Status,
    DateTime? LastSentAtUtc);

public sealed record AccountInvitationSummaryResponse(
    Guid TenantId,
    Guid InvitationId,
    string Email,
    string? FirstName,
    string? LastName,
    DateTime ExpiresAtUtc,
    string Status,
    int ResendCount,
    DateTime CreatedAtUtc,
    DateTime? LastSentAtUtc,
    DateTime? AcceptedAtUtc,
    DateTime? RevokedAtUtc,
    string? LastDeliveryStatus);

public sealed record AccountMemberResponse(
    Guid TenantId,
    Guid UserId,
    string UserName,
    string Email,
    string? FirstName,
    string? LastName,
    bool IsActive,
    IReadOnlyCollection<string> Roles,
    IReadOnlyCollection<string> Permissions,
    DateTime CreatedAt,
    DateTime? LastLoginAt);

public sealed record AccountRoleCatalogResponse(
    string Name,
    int Rank,
    bool IsProtected,
    IReadOnlyCollection<string> Permissions);

public sealed record UpdateAccountMemberRolesRequest(IReadOnlyCollection<string> Roles);
