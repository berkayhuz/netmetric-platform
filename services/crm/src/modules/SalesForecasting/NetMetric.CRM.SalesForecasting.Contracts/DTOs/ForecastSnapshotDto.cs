// <copyright file="ForecastSnapshotDto.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.SalesForecasting.Contracts.DTOs;

public sealed record ForecastSnapshotDto(
    Guid Id,
    string Name,
    DateOnly PeriodStart,
    DateOnly PeriodEnd,
    Guid? OwnerUserId,
    string ForecastCategory,
    decimal PipelineAmount,
    decimal WeightedPipelineAmount,
    decimal BestCaseAmount,
    decimal CommitAmount,
    decimal ClosedWonAmount,
    decimal QuotaAmount,
    decimal AdjustmentAmount,
    string? Notes,
    string RowVersion);
