// <copyright file="AiInsightRun.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.CRM.ArtificialIntelligence.Domain.Enums;
using NetMetric.Entities;

namespace NetMetric.CRM.ArtificialIntelligence.Domain.Entities;

public sealed class AiInsightRun : AuditableEntity
{
    private AiInsightRun()
    {
    }

    public AiInsightRun(Guid providerConnectionId, AiCapabilityType capability, string sourceEntityType, Guid sourceEntityId, AiRunStatus status, decimal? confidenceScore)
    {
        ProviderConnectionId = providerConnectionId;
        Capability = capability;
        SourceEntityType = string.IsNullOrWhiteSpace(sourceEntityType) ? throw new ArgumentException("Source entity type is required.", nameof(sourceEntityType)) : sourceEntityType.Trim();
        SourceEntityId = sourceEntityId;
        Status = status;
        ConfidenceScore = confidenceScore;
    }

    public Guid ProviderConnectionId { get; private set; }
    public AiCapabilityType Capability { get; private set; }
    public string SourceEntityType { get; private set; } = null!;
    public Guid SourceEntityId { get; private set; }
    public AiRunStatus Status { get; private set; }
    public decimal? ConfidenceScore { get; private set; }
}
