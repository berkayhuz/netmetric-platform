// <copyright file="InvitationRequests.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.Auth.Contracts.Requests;

public sealed record CreateTenantInvitationRequest(
    string Email,
    string? FirstName,
    string? LastName);

public sealed record AcceptTenantInvitationRequest(
    Guid TenantId,
    string Token,
    string UserName,
    string Email,
    string Password,
    string? FirstName,
    string? LastName);
