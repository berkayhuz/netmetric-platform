// <copyright file="OpportunityUpsertRequest.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.CRM.Types;

namespace NetMetric.CRM.OpportunityManagement.Contracts.Requests;

public sealed class OpportunityUpsertRequest
{
    public string OpportunityCode { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public decimal EstimatedAmount { get; set; }
    public decimal? ExpectedRevenue { get; set; }
    public decimal Probability { get; set; }
    public DateTime? EstimatedCloseDate { get; set; }
    public OpportunityStageType Stage { get; set; } = OpportunityStageType.Prospecting;
    public OpportunityStatusType Status { get; set; } = OpportunityStatusType.Open;
    public PriorityType Priority { get; set; } = PriorityType.Medium;
    public Guid? LeadId { get; set; }
    public Guid? CustomerId { get; set; }
    public Guid? OwnerUserId { get; set; }
    public string? Notes { get; set; }
    public string? RowVersion { get; set; }
}
