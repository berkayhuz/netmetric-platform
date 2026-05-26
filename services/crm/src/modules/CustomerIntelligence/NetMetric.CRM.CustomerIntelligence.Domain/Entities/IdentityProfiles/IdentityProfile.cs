// <copyright file="IdentityProfile.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.Entities;

namespace NetMetric.CRM.CustomerIntelligence.Domain.Entities.IdentityProfiles;

public sealed class IdentityProfile : AuditableEntity
{
    public string SubjectType { get; set; } = null!;
    public Guid SubjectId { get; set; }
    public string IdentityType { get; set; } = null!;
    public string IdentityValue { get; set; } = null!;
    public decimal ConfidenceScore { get; set; }
    public string? ResolutionNotes { get; set; }
    public DateTime LastResolvedAtUtc { get; set; } = DateTime.UtcNow;
}
