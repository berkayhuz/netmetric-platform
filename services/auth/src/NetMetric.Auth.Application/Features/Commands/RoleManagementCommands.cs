// <copyright file="RoleManagementCommands.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.Auth.Contracts.Responses;

namespace NetMetric.Auth.Application.Features.Commands;

public sealed record ListTenantMembersCommand(
    Guid TenantId,
    Guid RequestedByUserId) : IRequest<IReadOnlyCollection<TenantMemberResponse>>;

public sealed record UpdateTenantMemberRolesCommand(
    Guid TenantId,
    Guid TargetUserId,
    Guid RequestedByUserId,
    IReadOnlyCollection<string> Roles,
    string? IpAddress,
    string? UserAgent,
    string? CorrelationId,
    string? TraceId) : IRequest<TenantMemberResponse>;

public sealed record ListRoleCatalogCommand(
    Guid TenantId,
    Guid RequestedByUserId) : IRequest<IReadOnlyCollection<RoleCatalogResponse>>;
