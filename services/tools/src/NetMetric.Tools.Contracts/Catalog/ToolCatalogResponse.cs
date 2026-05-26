// <copyright file="ToolCatalogResponse.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.Tools.Contracts.Catalog;

public sealed record ToolCatalogResponse(
    IReadOnlyCollection<ToolCategoryResponse> Categories,
    IReadOnlyCollection<ToolCatalogItemResponse> Tools);
