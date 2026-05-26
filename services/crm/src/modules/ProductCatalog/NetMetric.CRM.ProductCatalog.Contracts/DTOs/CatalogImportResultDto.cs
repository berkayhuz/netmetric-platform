// <copyright file="CatalogImportResultDto.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.CRM.ProductCatalog.Contracts.DTOs;

public sealed class CatalogImportResultDto
{
    public required int RequestedCount { get; init; }
    public required int CreatedCount { get; init; }
    public required int UpdatedCount { get; init; }
    public required int SkippedCount { get; init; }
}
