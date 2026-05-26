// <copyright file="LeadScore.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.Sales;

public class LeadScore : AuditableEntity
{
    public Guid LeadId { get; set; }
    public Lead? Lead { get; set; }
    public decimal Score { get; set; }
    public decimal FitScoreDelta { get; set; }
    public decimal EngagementScoreDelta { get; set; }
    public string? ScoreReason { get; set; }
    public string? RuleId { get; set; }
    public DateTime CalculatedAt { get; set; }
}
