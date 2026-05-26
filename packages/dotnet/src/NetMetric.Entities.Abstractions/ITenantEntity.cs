// <copyright file="ITenantEntity.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.Entities.Abstractions;

public interface ITenantEntity
{
    Guid TenantId { get; set; }
}
