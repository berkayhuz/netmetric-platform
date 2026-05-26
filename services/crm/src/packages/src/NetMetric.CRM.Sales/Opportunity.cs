// <copyright file="Opportunity.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.Sales;

public class Opportunity : AuditableEntity
{
    public string OpportunityCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal EstimatedAmount { get; set; }
    public decimal? ExpectedRevenue { get; set; }
    public decimal Probability { get; set; }
    public OpportunityStageType Stage { get; set; }
    public Guid? PipelineId { get; set; }
    public Guid? PipelineStageId { get; set; }
    public OpportunityStatusType Status { get; set; }
    public ForecastCategory ForecastCategory { get; set; }
    public PriorityType Priority { get; set; }
    public Guid? OwnerUserId { get; set; }
    public Guid? LeadId { get; set; }
    public Lead? Lead { get; set; }
    public Guid? CustomerId { get; set; }
    public Customer? Customer { get; set; }
    public Guid? CompanyId { get; set; }
    public Company? Company { get; set; }
    public Guid? LostReasonId { get; set; }
    public LostReason? LostReason { get; set; }
    public string? LostNote { get; set; }
    public DateTime? ExpectedCloseDate { get; set; }
    public DateTime? EstimatedCloseDate { get; set; }
    public DateTime? ClosedAt { get; set; }
    public string? Description { get; set; }
    public string? Notes { get; set; }
    public ICollection<OpportunityProduct> Products { get; set; } = [];
    public ICollection<OpportunityContact> Contacts { get; set; } = [];
    public ICollection<Quote> Quotes { get; set; } = [];

    public void SetNotes(string? notes) => Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
}
