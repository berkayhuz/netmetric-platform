// <copyright file="IdentityResolutionRequest.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.CustomerIntelligence.Contracts.Requests;

public sealed class IdentityResolutionRequest
{
    public string SubjectType { get; set; } = "Customer";
    public Guid SubjectId { get; set; }
    public string IdentityType { get; set; } = null!;
    public string IdentityValue { get; set; } = null!;
    public decimal ConfidenceScore { get; set; }
    public string? ResolutionNotes { get; set; }
}
