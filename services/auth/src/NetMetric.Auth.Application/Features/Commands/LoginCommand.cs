// <copyright file="LoginCommand.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.Auth.Contracts.Responses;

namespace NetMetric.Auth.Application.Features.Commands;

public sealed record LoginCommand(
    Guid TenantId,
    string EmailOrUserName,
    string Password,
    bool RememberMe,
    string? MfaCode,
    string? RecoveryCode,
    string? IpAddress,
    string? UserAgent) : IRequest<AuthenticationTokenResponse>;
