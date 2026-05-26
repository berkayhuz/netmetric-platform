// <copyright file="PipelineSnapshot.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.Entities;

namespace NetMetric.CRM.PipelineManagement.Domain.Entities;

public class PipelineSnapshot : AuditableEntity
{
    public DateTime SnapshotDate { get; set; }
    public Guid PipelineId { get; set; }

    public decimal TotalValue { get; set; }
    public decimal WeightedValue { get; set; }
    public int OpportunityCount { get; set; }

    public string DataJson { get; set; } = string.Empty; // Aggregated data per stage
}
