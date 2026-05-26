// <copyright file="Customer360Mappings.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.CRM.CustomerIntelligence.Contracts.DTOs;
using NetMetric.CRM.CustomerIntelligence.Domain.Entities.BehavioralEvents;
using NetMetric.CRM.CustomerIntelligence.Domain.Entities.CustomerRelationships;
using NetMetric.CRM.CustomerIntelligence.Domain.Entities.CustomerTimelineEntrys;
using NetMetric.CRM.CustomerIntelligence.Domain.Entities.IdentityProfiles;

namespace NetMetric.CRM.CustomerIntelligence.Application.Features.Customer360;

internal static class Customer360Mappings
{
    public static Customer360ActivityDto ToDto(this CustomerTimelineEntry entity)
        => new(entity.Id, entity.SubjectType, entity.SubjectId, entity.Name, entity.Category, entity.Channel, entity.EntityType, entity.RelatedEntityId, entity.DataJson, entity.OccurredAtUtc);

    public static RelationshipEdgeDto ToDto(this CustomerRelationship entity)
        => new(
            entity.Id,
            entity.Name,
            entity.RelationshipType,
            new RelationshipNodeDto(entity.SourceEntityType, entity.SourceEntityId),
            new RelationshipNodeDto(entity.TargetEntityType, entity.TargetEntityId),
            entity.StrengthScore,
            entity.IsBidirectional,
            entity.OccurredAtUtc,
            entity.DataJson);

    public static BehavioralEventDto ToDto(this BehavioralEvent entity)
        => new(entity.Id, entity.Source, entity.EventName, entity.SubjectType, entity.SubjectId, entity.IdentityKey, entity.Channel, entity.PropertiesJson, entity.OccurredAtUtc);

    public static IdentityResolutionDto ToDto(this IdentityProfile entity)
        => new(entity.Id, entity.SubjectType, entity.SubjectId, entity.IdentityType, entity.IdentityValue, entity.ConfidenceScore, entity.ResolutionNotes, entity.LastResolvedAtUtc);
}
