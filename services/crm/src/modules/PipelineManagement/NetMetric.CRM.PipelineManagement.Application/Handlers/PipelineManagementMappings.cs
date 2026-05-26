// <copyright file="PipelineManagementMappings.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.CRM.PipelineManagement.Contracts.DTOs;
using NetMetric.CRM.PipelineManagement.Domain.Entities;
using NetMetric.CRM.Sales;

namespace NetMetric.CRM.PipelineManagement.Application.Handlers;

public static class PipelineManagementMappings
{
    public static LostReasonDto ToDto(this LostReason entity)
        => new(entity.Id, entity.Name, entity.Description, entity.IsDefault, Convert.ToBase64String(entity.RowVersion));

    public static OpportunityStageHistoryDto ToDto(this OpportunityStageHistory entity)
        => new(entity.Id, entity.OpportunityId, entity.OldStage, entity.NewStage, entity.ChangedAt, entity.ChangedByUserId, entity.Note);

    public static PipelineDto ToDto(this Pipeline entity)
        => new(
            entity.Id,
            entity.Name,
            entity.Description,
            entity.IsDefault,
            entity.DisplayOrder,
            entity.Stages.Select(ToDto).ToList(),
            Convert.ToBase64String(entity.RowVersion));

    public static PipelineStageDto ToDto(this PipelineStage entity)
        => new(
            entity.Id,
            entity.PipelineId,
            entity.Name,
            entity.Description,
            entity.DisplayOrder,
            entity.Probability,
            entity.IsWinStage,
            entity.IsLostStage,
            entity.ForecastCategory,
            entity.RequiredFields.Select(ToDto).ToList(),
            entity.ExitCriteria.Select(ToDto).ToList(),
            Convert.ToBase64String(entity.RowVersion));

    public static StageRequiredFieldDto ToDto(this StageRequiredField entity)
        => new(entity.Id, entity.FieldName, entity.DisplayName, entity.ValidationRule, entity.ErrorMessage);

    public static StageExitCriteriaDto ToDto(this StageExitCriteria entity)
        => new(entity.Id, entity.Name, entity.Description, entity.IsMandatory);
}
