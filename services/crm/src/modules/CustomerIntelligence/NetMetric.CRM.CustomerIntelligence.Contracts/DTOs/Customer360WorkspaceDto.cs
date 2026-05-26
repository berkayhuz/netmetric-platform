// <copyright file="Customer360WorkspaceDto.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.CustomerIntelligence.Contracts.DTOs;

public sealed record Customer360WorkspaceDto(
    Guid CustomerId,
    IReadOnlyList<Customer360ActivityDto> ActivityStream,
    IReadOnlyList<RelationshipEdgeDto> RelationshipGraph,
    IReadOnlyList<BehavioralEventDto> RecentBehavioralEvents,
    IReadOnlyList<IdentityResolutionDto> LinkedIdentities);
