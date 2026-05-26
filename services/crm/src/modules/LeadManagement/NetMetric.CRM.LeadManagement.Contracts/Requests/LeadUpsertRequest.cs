// <copyright file="LeadUpsertRequest.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.CRM.Types;

namespace NetMetric.CRM.LeadManagement.Contracts.Requests;

public sealed class LeadUpsertRequest
{
    public string FullName { get; set; } = null!;
    public string? CompanyName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? JobTitle { get; set; }
    public string? Description { get; set; }
    public decimal? EstimatedBudget { get; set; }
    public DateTime? NextContactDate { get; set; }
    public LeadSourceType Source { get; set; } = LeadSourceType.Manual;
    public LeadStatusType Status { get; set; } = LeadStatusType.New;
    public PriorityType Priority { get; set; } = PriorityType.Medium;
    public Guid? CompanyId { get; set; }
    public Guid? OwnerUserId { get; set; }
    public string? Notes { get; set; }
    public string? RowVersion { get; set; }
}
