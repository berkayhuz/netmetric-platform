// <copyright file="CatalogBulkOperationResultDto.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.ProductCatalog.Contracts.DTOs;

public sealed class CatalogBulkOperationResultDto
{
    public required int RequestedCount { get; init; }
    public required int ProcessedCount { get; init; }
}
