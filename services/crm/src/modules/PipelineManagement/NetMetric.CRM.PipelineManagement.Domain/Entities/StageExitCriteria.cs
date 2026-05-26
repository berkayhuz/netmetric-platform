// <copyright file="StageExitCriteria.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.Entities;

namespace NetMetric.CRM.PipelineManagement.Domain.Entities;

public class StageExitCriteria : AuditableEntity
{
    public Guid PipelineStageId { get; set; }
    public PipelineStage? PipelineStage { get; set; }

    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsMandatory { get; set; } = true;
}
