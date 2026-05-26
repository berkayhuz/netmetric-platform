// <copyright file="SalesForecastSummaryDto.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.SalesForecasting.Contracts.DTOs;

public sealed record SalesForecastSummaryDto(
    DateOnly PeriodStart,
    DateOnly PeriodEnd,
    decimal PipelineAmount,
    decimal WeightedPipelineAmount,
    decimal BestCaseAmount,
    decimal CommitAmount,
    decimal ClosedWonAmount,
    decimal QuotaAmount,
    decimal AdjustmentAmount,
    decimal CoverageRatio,
    decimal AttainmentRatio,
    IReadOnlyList<SalesForecastOwnerSummaryDto> Owners);
