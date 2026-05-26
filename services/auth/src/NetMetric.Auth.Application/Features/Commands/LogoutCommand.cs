// <copyright file="LogoutCommand.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;

namespace NetMetric.Auth.Application.Features.Commands;

public sealed record LogoutCommand(
    Guid TenantId,
    Guid SessionId,
    string RefreshToken) : IRequest<Unit>;
