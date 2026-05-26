// <copyright file="IdentityResolutionDto.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.CustomerIntelligence.Contracts.DTOs;

public sealed record IdentityResolutionDto(
    Guid Id,
    string SubjectType,
    Guid SubjectId,
    string IdentityType,
    string IdentityValue,
    decimal ConfidenceScore,
    string? ResolutionNotes,
    DateTime LastResolvedAtUtc);
