// <copyright file="ActivityParticipant.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.Activities;

public class ActivityParticipant : AuditableEntity
{
    public Guid ActivityId { get; set; }
    public Activity? Activity { get; set; }
    public Guid ParticipantId { get; set; }
}
