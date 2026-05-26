// <copyright file="Pipeline.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.Entities;

namespace NetMetric.CRM.PipelineManagement.Domain.Entities;

public class Pipeline : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsDefault { get; set; }
    public int DisplayOrder { get; set; }

    public ICollection<PipelineStage> Stages { get; set; } = [];
}
