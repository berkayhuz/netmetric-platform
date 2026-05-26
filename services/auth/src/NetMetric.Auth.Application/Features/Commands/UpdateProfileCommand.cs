// <copyright file="UpdateProfileCommand.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.Auth.Contracts.Responses;

namespace NetMetric.Auth.Application.Features.Commands;

public sealed record UpdateProfileCommand(
    Guid TenantId,
    Guid UserId,
    string? FirstName,
    string? LastName) : IRequest<UserProfileResponse>;
