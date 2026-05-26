// <copyright file="LeadOwnershipHistory.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.Sales;

public class LeadOwnershipHistory : AuditableEntity
{
    public Guid LeadId { get; set; }
    public Lead? Lead { get; set; }
    public Guid? PreviousOwnerId { get; set; }
    public Guid? NewOwnerId { get; set; }
    public string? AssignmentReason { get; set; }
    public string? AssignmentRuleId { get; set; }
    public DateTime AssignedAt { get; set; }
    public Guid? AssignedByUserId { get; set; }
}
