// <copyright file="SearchReindexRequestedV1.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.Search.Contracts.Documents;

namespace NetMetric.Search.Contracts.IntegrationEvents.V1;

public sealed record SearchReindexRequestedV1(
    Guid EventId,
    SearchDocumentSource? Source,
    Guid? TenantId,
    string? RequestedBy,
    string? CorrelationId,
    string? CausationId,
    DateTime OccurredAtUtc)
{
    public const string EventName = "search.reindex.requested";
    public const int EventVersion = 1;
    public const string RoutingKey = "search.reindex.requested.v1";
}
