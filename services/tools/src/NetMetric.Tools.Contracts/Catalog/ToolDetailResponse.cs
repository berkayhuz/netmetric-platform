// <copyright file="ToolDetailResponse.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.Tools.Contracts.Catalog;

public sealed record ToolDetailResponse(
    string Slug,
    string Title,
    string Description,
    string CategorySlug,
    string ExecutionMode,
    string AvailabilityStatus,
    bool IsEnabled,
    IReadOnlyCollection<string> AcceptedMimeTypes,
    long GuestMaxFileBytes,
    long AuthenticatedMaxSaveBytes,
    string SeoTitle,
    string SeoDescription);
