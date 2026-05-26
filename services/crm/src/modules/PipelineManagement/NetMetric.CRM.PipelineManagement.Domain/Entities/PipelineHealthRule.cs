// <copyright file="PipelineHealthRule.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.CRM.Types;
using NetMetric.Entities;

namespace NetMetric.CRM.PipelineManagement.Domain.Entities;

public class PipelineHealthRule : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string RuleType { get; set; } = string.Empty; // e.g. "Stale", "NoActivity", "PastCloseDate"
    public int ThresholdDays { get; set; }
    public PipelineHealthStatus Severity { get; set; } = PipelineHealthStatus.AtRisk;
}
