// <copyright file="ToolHistoryQuery.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.Tools.Contracts.History;

public sealed record ToolHistoryQuery(int Page = 1, int PageSize = 20, string? ToolSlug = null);
