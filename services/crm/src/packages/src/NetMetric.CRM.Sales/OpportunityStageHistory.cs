// <copyright file="OpportunityStageHistory.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.Sales;

public class OpportunityStageHistory : AuditableEntity
{
    public Guid OpportunityId { get; set; }
    public Opportunity? Opportunity { get; set; }
    public OpportunityStageType OldStage { get; set; }
    public OpportunityStageType NewStage { get; set; }
    public string? Note { get; set; }
    public DateTime OccurredAt { get; set; }
    public DateTime ChangedAt { get; set; }
    public Guid? ChangedByUserId { get; set; }
}
