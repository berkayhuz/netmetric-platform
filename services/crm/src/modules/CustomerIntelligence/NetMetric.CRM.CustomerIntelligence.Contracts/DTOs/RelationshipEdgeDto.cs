// <copyright file="RelationshipEdgeDto.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.CustomerIntelligence.Contracts.DTOs;

public sealed record RelationshipEdgeDto(
    Guid Id,
    string Name,
    string RelationshipType,
    RelationshipNodeDto Source,
    RelationshipNodeDto Target,
    decimal StrengthScore,
    bool IsBidirectional,
    DateTime OccurredAtUtc,
    string? DataJson);
