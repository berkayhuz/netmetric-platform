// <copyright file="RelationshipNodeDto.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.CustomerIntelligence.Contracts.DTOs;

public sealed record RelationshipNodeDto(string EntityType, Guid EntityId);
