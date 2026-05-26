// <copyright file="DealOutcomeHistory.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.Entities;

namespace NetMetric.CRM.DealManagement.Domain.Entities;

public class DealOutcomeHistory : EntityBase
{
    public Guid DealId { get; set; }
    public string Outcome { get; set; } = null!;
    public string Stage { get; set; } = null!;
    public DateTime OccurredAt { get; set; }
    public Guid? ChangedByUserId { get; set; }
    public Guid? LostReasonId { get; set; }
    public string? Note { get; set; }
}
