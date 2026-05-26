// <copyright file="SalesForecastWorkspaceDto.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.SalesForecasting.Contracts.DTOs;

public sealed record SalesForecastWorkspaceDto(
    SalesForecastSummaryDto Summary,
    IReadOnlyList<OpportunityForecastRowDto> Opportunities,
    IReadOnlyList<SalesQuotaDto> Quotas,
    IReadOnlyList<ForecastAdjustmentDto> Adjustments,
    IReadOnlyList<ForecastSnapshotDto> Snapshots);
