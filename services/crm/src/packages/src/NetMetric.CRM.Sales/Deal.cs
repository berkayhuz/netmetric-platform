// <copyright file="Deal.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.Sales;

public class Deal : AuditableEntity
{
    public string DealCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public decimal Amount { get; set; }
    public string? Stage { get; set; }
    public string? Outcome { get; set; }
    public Guid? OwnerUserId { get; set; }
    public Guid? OpportunityId { get; set; }
    public Opportunity? Opportunity { get; set; }
    public Guid? CustomerId { get; set; }
    public Customer? Customer { get; set; }
    public Guid? CompanyId { get; set; }
    public Company? Company { get; set; }
    public Guid? LostReasonId { get; set; }
    public LostReason? LostReason { get; set; }
    public string? LostNote { get; set; }
    public DateTime ClosedDate { get; set; }
    public DateTime? ClosedAt { get; set; }
    public string? Notes { get; set; }

    public void SetNotes(string? notes) => Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
}
