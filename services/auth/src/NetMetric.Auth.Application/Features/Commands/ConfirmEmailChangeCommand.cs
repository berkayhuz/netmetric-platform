// <copyright file="ConfirmEmailChangeCommand.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;

namespace NetMetric.Auth.Application.Features.Commands;

public sealed record ConfirmEmailChangeCommand(
    Guid TenantId,
    Guid UserId,
    string NewEmail,
    string Token,
    string? IpAddress,
    string? UserAgent,
    string? CorrelationId,
    string? TraceId) : IRequest;
