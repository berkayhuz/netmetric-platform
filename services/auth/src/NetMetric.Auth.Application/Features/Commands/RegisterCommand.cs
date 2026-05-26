// <copyright file="RegisterCommand.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.Auth.Application.Records;

namespace NetMetric.Auth.Application.Features.Commands;

public sealed record RegisterCommand(
    string TenantName,
    string UserName,
    string Email,
    string Password,
    string? FirstName,
    string? LastName,
    string? Culture,
    string? IpAddress,
    string? UserAgent) : IRequest<AuthSessionResult>;
