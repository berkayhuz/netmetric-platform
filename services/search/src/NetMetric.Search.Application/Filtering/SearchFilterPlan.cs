// <copyright file="SearchFilterPlan.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.Search.Contracts.Documents;

namespace NetMetric.Search.Application.Filtering;

public sealed record SearchFilterPlan(
    string Query,
    int Page,
    int PageSize,
    bool IncludeDeleted,
    bool IsAuthenticated,
    Guid? TenantId,
    IReadOnlyCollection<string> UserPermissions,
    bool HasWildcardPermission,
    IReadOnlyCollection<SearchDocumentSource> AllowedSources,
    IReadOnlyCollection<SearchDocumentSource> RequestedSources,
    IReadOnlyCollection<SearchDocumentSource> EffectiveSources,
    string? Type,
    string? Locale,
    IReadOnlyCollection<string> Tags,
    string? Sort,
    bool AllowPublicVisibility,
    bool AllowAuthenticatedVisibility,
    bool AllowTenantVisibility,
    bool AllowTenantNavigationWithoutTenantId,
    bool AllowPermissionVisibility,
    bool RequiresPermissionPostFiltering,
    bool RequiresVisibilityPostFiltering);
