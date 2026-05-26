// <copyright file="OpportunityLifecycleIntegrationEvents.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.OpportunityManagement.Application.IntegrationEvents;

public sealed record OpportunityLifecycleIntegrationEventV1(
    Guid EventId,
    Guid TenantId,
    Guid EntityId,
    string EntityType,
    string EventType,
    Guid? OwnerUserId,
    IReadOnlyDictionary<string, string> Metadata,
    string? CorrelationId,
    DateTimeOffset OccurredAtUtc)
{
    public const int EventVersion = 1;
}

public static class OpportunityManagementIntegrationEventNames
{
    public const string OpportunityPurged = "crm.opportunity.purged";
}
