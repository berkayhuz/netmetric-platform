// <copyright file="ToolCategoryResponse.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.Tools.Contracts.Catalog;

public sealed record ToolCategoryResponse(
    string Slug,
    string Title,
    string Description,
    int SortOrder);
