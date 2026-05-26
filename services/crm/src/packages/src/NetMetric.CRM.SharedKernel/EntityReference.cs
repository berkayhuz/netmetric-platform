// <copyright file="EntityReference.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.SharedKernel;

public readonly record struct EntityReference(string EntityType, Guid EntityId);
