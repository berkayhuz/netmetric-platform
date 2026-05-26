// <copyright file="MeilisearchDocumentMapper.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.Search.Application.Security;
using NetMetric.Search.Application.Queries;
using NetMetric.Search.Contracts.Documents;

namespace NetMetric.Search.Infrastructure.Meilisearch.Documents;

internal sealed class MeilisearchDocumentMapper
{
    public MeilisearchSearchDocument ToStoredDocument(SearchDocument document)
    {
        var validationErrors = SearchDocumentSecurityValidator.Validate(document);
        if (validationErrors.Count > 0)
        {
            throw new InvalidOperationException(
                $"Cannot map invalid search document '{document.Id}': {string.Join("; ", validationErrors)}");
        }

        return new MeilisearchSearchDocument
        {
            Id = document.Id,
            Source = document.Source.ToString(),
            Type = document.Type,
            Title = document.Title,
            Summary = document.Summary,
            Content = document.Content,
            Url = document.Url,
            TenantId = document.TenantId,
            RequiredPermissions = NormalizeStringList(document.RequiredPermissions),
            Visibility = document.Visibility.ToString(),
            PermissionMatchMode = document.PermissionMatchMode.ToString(),
            Locale = document.Locale,
            Tags = NormalizeStringList(document.Tags),
            Boost = document.Boost,
            CreatedAtUtc = document.CreatedAtUtc,
            UpdatedAtUtc = document.UpdatedAtUtc,
            IndexedAtUtc = document.IndexedAtUtc,
            IsDeleted = document.IsDeleted,
            Metadata = NormalizeMetadata(document.Metadata),
            ExternalId = document.ExternalId,
            OwnerUserId = document.OwnerUserId,
            AssignedUserIds = NormalizeGuidList(document.AssignedUserIds),
            SourceVersion = document.SourceVersion,
            SourceUpdatedAtUtc = document.SourceUpdatedAtUtc
        };
    }

    public SearchDocument ToSearchDocument(MeilisearchSearchDocument document)
    {
        var source = ParseEnum<SearchDocumentSource>(document.Source, nameof(document.Source));
        var visibility = ParseEnum<SearchDocumentVisibility>(document.Visibility, nameof(document.Visibility));
        var permissionMatchMode = ParseEnum<SearchPermissionMatchMode>(document.PermissionMatchMode, nameof(document.PermissionMatchMode));

        return new SearchDocument(
            Id: document.Id,
            Source: source,
            Type: document.Type,
            Title: document.Title,
            Summary: document.Summary,
            Content: document.Content,
            Url: document.Url,
            TenantId: document.TenantId,
            RequiredPermissions: NormalizeStringList(document.RequiredPermissions),
            Visibility: visibility,
            Locale: document.Locale,
            Tags: NormalizeStringList(document.Tags),
            Boost: document.Boost,
            CreatedAtUtc: document.CreatedAtUtc,
            UpdatedAtUtc: document.UpdatedAtUtc,
            IndexedAtUtc: document.IndexedAtUtc,
            IsDeleted: document.IsDeleted,
            Metadata: NormalizeMetadata(document.Metadata),
            ExternalId: document.ExternalId,
            OwnerUserId: document.OwnerUserId,
            AssignedUserIds: NormalizeGuidList(document.AssignedUserIds),
            SourceVersion: document.SourceVersion,
            SourceUpdatedAtUtc: document.SourceUpdatedAtUtc,
            PermissionMatchMode: permissionMatchMode);
    }

    public SearchResultItem ToSearchResultItem(MeilisearchSearchDocument document)
    {
        var source = ParseEnum<SearchDocumentSource>(document.Source, nameof(document.Source));
        var visibility = ParseEnum<SearchDocumentVisibility>(document.Visibility, nameof(document.Visibility));

        return new SearchResultItem(
            Id: document.Id,
            Source: source,
            Type: document.Type,
            Title: document.Title,
            Summary: document.Summary,
            Url: document.Url,
            Visibility: visibility,
            Locale: document.Locale,
            Tags: NormalizeStringList(document.Tags),
            RankingScore: document.RankingScore);
    }

    private static TEnum ParseEnum<TEnum>(string rawValue, string fieldName)
        where TEnum : struct, Enum
    {
        if (Enum.TryParse<TEnum>(rawValue, true, out var parsed))
        {
            return parsed;
        }

        throw new InvalidOperationException($"Invalid {fieldName} value '{rawValue}'.");
    }

    private static IReadOnlyCollection<string> NormalizeStringList(IReadOnlyCollection<string>? values)
    {
        if (values is null || values.Count == 0)
        {
            return [];
        }

        return values
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static IReadOnlyCollection<Guid> NormalizeGuidList(IReadOnlyCollection<Guid>? values)
    {
        if (values is null || values.Count == 0)
        {
            return [];
        }

        return values.Distinct().ToArray();
    }

    private static IReadOnlyDictionary<string, string> NormalizeMetadata(IReadOnlyDictionary<string, string>? metadata)
    {
        if (metadata is null || metadata.Count == 0)
        {
            return new Dictionary<string, string>(StringComparer.Ordinal);
        }

        return metadata
            .Where(kvp => !string.IsNullOrWhiteSpace(kvp.Key))
            .ToDictionary(
                kvp => kvp.Key.Trim(),
                kvp => kvp.Value ?? string.Empty,
                StringComparer.OrdinalIgnoreCase);
    }
}
