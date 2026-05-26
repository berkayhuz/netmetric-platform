// <copyright file="BehavioralEventTrackRequest.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.CustomerIntelligence.Contracts.Requests;

public sealed class BehavioralEventTrackRequest
{
    public string Source { get; set; } = null!;
    public string EventName { get; set; } = null!;
    public string SubjectType { get; set; } = "Customer";
    public Guid SubjectId { get; set; }
    public string? IdentityKey { get; set; }
    public string? Channel { get; set; }
    public string? PropertiesJson { get; set; }
    public DateTime? OccurredAtUtc { get; set; }
}
