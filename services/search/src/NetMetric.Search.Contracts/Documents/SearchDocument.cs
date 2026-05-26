// <copyright file="SearchDocument.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

namespace NetMetric.Search.Contracts.Documents;

public sealed record SearchDocument(
    string Id,
    SearchDocumentSource Source,
    string Type,
    string Title,
    string Summary,
    string Content,
    string Url,
    Guid? TenantId,
    IReadOnlyCollection<string> RequiredPermissions,
    SearchDocumentVisibility Visibility,
    string Locale,
    IReadOnlyCollection<string> Tags,
    double Boost,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc,
    DateTimeOffset IndexedAtUtc,
    bool IsDeleted,
    IReadOnlyDictionary<string, string> Metadata,
    string? ExternalId = null,
    Guid? OwnerUserId = null,
    IReadOnlyCollection<Guid>? AssignedUserIds = null,
    string? SourceVersion = null,
    DateTimeOffset? SourceUpdatedAtUtc = null,
    SearchPermissionMatchMode PermissionMatchMode = SearchPermissionMatchMode.Any);
