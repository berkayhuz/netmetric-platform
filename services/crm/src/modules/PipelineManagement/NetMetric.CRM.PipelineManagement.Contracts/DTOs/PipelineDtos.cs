// <copyright file="PipelineDtos.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.CRM.Types;

namespace NetMetric.CRM.PipelineManagement.Contracts.DTOs;

public record PipelineDto(
    Guid Id,
    string Name,
    string? Description,
    bool IsDefault,
    int DisplayOrder,
    List<PipelineStageDto> Stages,
    string RowVersion);

public record PipelineStageDto(
    Guid Id,
    Guid PipelineId,
    string Name,
    string? Description,
    int DisplayOrder,
    decimal Probability,
    bool IsWinStage,
    bool IsLostStage,
    ForecastCategory ForecastCategory,
    List<StageRequiredFieldDto> RequiredFields,
    List<StageExitCriteriaDto> ExitCriteria,
    string RowVersion);

public record StageRequiredFieldDto(
    Guid Id,
    string FieldName,
    string? DisplayName,
    string? ValidationRule,
    string? ErrorMessage);

public record StageExitCriteriaDto(
    Guid Id,
    string Name,
    string? Description,
    bool IsMandatory);

public record PipelineSummaryDto(
    Guid Id,
    string Name,
    int StageCount,
    bool IsDefault);

public record PipelineBoardDto(
    Guid PipelineId,
    string PipelineName,
    List<PipelineBoardColumnDto> Columns);

public record PipelineBoardColumnDto(
    Guid StageId,
    string Name,
    decimal Probability,
    int OpportunityCount,
    decimal TotalValue,
    List<OpportunitySummaryDto> Opportunities);

public record OpportunitySummaryDto(
    Guid Id,
    string Name,
    string OpportunityCode,
    decimal Amount,
    string? CustomerName,
    DateTime? EstimatedCloseDate,
    bool IsStale,
    int WarningCount);
