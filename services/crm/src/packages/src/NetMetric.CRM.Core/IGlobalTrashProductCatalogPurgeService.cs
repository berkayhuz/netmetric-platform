// <copyright file="IGlobalTrashProductCatalogPurgeService.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.Core;

public interface IGlobalTrashProductCatalogPurgeService
{
    Task<int> PurgeCatalogProductFromTrashAsync(GlobalTrashItem trashItem, DateTime nowUtc, CancellationToken cancellationToken = default);
}
