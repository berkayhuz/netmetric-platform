// <copyright file="PipelineAnalyticsDtos.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.PipelineManagement.Contracts.DTOs;

public record PipelineAnalyticsDto(
    Guid PipelineId,
    decimal HealthScore,
    decimal VelocityDays,
    decimal CoverageRatio,
    int TotalOpportunities,
    decimal TotalValue,
    List<StageAgingDto> StageAging);

public record StageAgingDto(
    Guid StageId,
    string StageName,
    int OpportunityCount,
    double AverageDaysInStage,
    int StaleCount);

public record PipelineSnapshotDto(
    DateTime SnapshotDate,
    int OpportunityCount,
    decimal TotalValue,
    int WonCount,
    int LostCount);
