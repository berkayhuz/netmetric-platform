// <copyright file="LeadScoreDto.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.LeadManagement.Contracts.DTOs;

public sealed record LeadScoreDto(
    Guid Id,
    decimal Score,
    string? ScoreReason,
    DateTime CalculatedAt);
