// <copyright file="ICatalogItemEntity.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.ProductCatalog.Domain.Common;

public interface ICatalogItemEntity
{
    string Code { get; }
    string Name { get; }
    string? Description { get; }
    bool IsActive { get; }

    void Update(string code, string name, string? description);
    void Activate();
    void Deactivate();
}
