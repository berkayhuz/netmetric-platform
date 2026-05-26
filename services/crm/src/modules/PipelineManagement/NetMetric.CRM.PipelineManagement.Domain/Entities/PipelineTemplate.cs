// <copyright file="PipelineTemplate.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.Entities;

namespace NetMetric.CRM.PipelineManagement.Domain.Entities;

public class PipelineTemplate : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Industry { get; set; }
    public string ConfigurationJson { get; set; } = string.Empty; // Store stages and rules as JSON
}
