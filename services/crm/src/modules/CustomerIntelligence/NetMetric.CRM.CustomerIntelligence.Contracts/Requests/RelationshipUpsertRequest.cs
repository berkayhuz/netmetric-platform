// <copyright file="RelationshipUpsertRequest.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.CustomerIntelligence.Contracts.Requests;

public sealed class RelationshipUpsertRequest
{
    public string SourceEntityType { get; set; } = "Customer";
    public Guid SourceEntityId { get; set; }
    public string TargetEntityType { get; set; } = "Customer";
    public Guid TargetEntityId { get; set; }
    public string Name { get; set; } = null!;
    public string RelationshipType { get; set; } = "Related";
    public decimal StrengthScore { get; set; }
    public bool IsBidirectional { get; set; } = true;
    public string? DataJson { get; set; }
}
