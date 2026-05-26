// <copyright file="ResolveIdentityCommand.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.CustomerIntelligence.Contracts.DTOs;

namespace NetMetric.CRM.CustomerIntelligence.Application.Features.Cdp.Commands.ResolveIdentity;

public sealed record ResolveIdentityCommand(
    string SubjectType,
    Guid SubjectId,
    string IdentityType,
    string IdentityValue,
    decimal ConfidenceScore,
    string? ResolutionNotes) : IRequest<IdentityResolutionDto>;
