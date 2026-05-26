// <copyright file="LeadLifecycleIntegrationEvents.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.LeadManagement.Application.IntegrationEvents;

public sealed record LeadLifecycleIntegrationEventV1(
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

public static class LeadManagementIntegrationEventNames
{
    public const string LeadPurged = "crm.lead.purged";
}
