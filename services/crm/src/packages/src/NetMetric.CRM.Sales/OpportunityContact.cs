// <copyright file="OpportunityContact.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.Sales;

public class OpportunityContact : AuditableEntity
{
    public Guid OpportunityId { get; set; }
    public Opportunity? Opportunity { get; set; }
    public Guid ContactId { get; set; }
    public Contact? Contact { get; set; }
    public string? Role { get; set; }
    public bool IsPrimary { get; set; }
    public bool IsDecisionMaker { get; set; }
}
