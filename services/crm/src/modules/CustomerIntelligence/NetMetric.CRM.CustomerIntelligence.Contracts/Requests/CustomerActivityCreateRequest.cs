// <copyright file="CustomerActivityCreateRequest.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.CustomerIntelligence.Contracts.Requests;

public sealed class CustomerActivityCreateRequest
{
    public string SubjectType { get; set; } = "Customer";
    public Guid SubjectId { get; set; }
    public string Name { get; set; } = null!;
    public string Category { get; set; } = "General";
    public string? Channel { get; set; }
    public string? EntityType { get; set; }
    public Guid? RelatedEntityId { get; set; }
    public string? DataJson { get; set; }
    public DateTime? OccurredAtUtc { get; set; }
}
