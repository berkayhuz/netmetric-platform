// <copyright file="ForecastSnapshot.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.Entities;

namespace NetMetric.CRM.SalesForecasting.Domain.Entities;

public class ForecastSnapshot : AuditableEntity
{
    public string Name { get; set; } = null!;
    public DateOnly PeriodStart { get; set; }
    public DateOnly PeriodEnd { get; set; }
    public Guid? OwnerUserId { get; set; }
    public string ForecastCategory { get; set; } = "Pipeline";
    public decimal PipelineAmount { get; set; }
    public decimal WeightedPipelineAmount { get; set; }
    public decimal BestCaseAmount { get; set; }
    public decimal CommitAmount { get; set; }
    public decimal ClosedWonAmount { get; set; }
    public decimal QuotaAmount { get; set; }
    public decimal AdjustmentAmount { get; set; }
    public string? Notes { get; set; }

    public decimal ForecastAmount => WeightedPipelineAmount + AdjustmentAmount;

    public void SetNotes(string? notes) => Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
}
