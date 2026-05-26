// <copyright file="PipelineStage.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.CRM.Types;
using NetMetric.Entities;

namespace NetMetric.CRM.PipelineManagement.Domain.Entities;

public class PipelineStage : AuditableEntity
{
    public Guid PipelineId { get; set; }
    public Pipeline? Pipeline { get; set; }

    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int DisplayOrder { get; set; }
    public decimal Probability { get; set; }

    public bool IsWinStage { get; set; }
    public bool IsLostStage { get; set; }

    public ForecastCategory ForecastCategory { get; set; } = ForecastCategory.Pipeline;

    public ICollection<StageRequiredField> RequiredFields { get; set; } = [];
    public ICollection<StageExitCriteria> ExitCriteria { get; set; } = [];
}
