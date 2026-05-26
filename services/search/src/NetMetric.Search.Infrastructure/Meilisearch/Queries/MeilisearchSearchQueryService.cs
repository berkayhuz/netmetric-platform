// <copyright file="MeilisearchSearchQueryService.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NetMetric.Search.Application.Abstractions;
using NetMetric.Search.Application.Filtering;
using NetMetric.Search.Application.Queries;
using NetMetric.Search.Application.Security;
using NetMetric.Search.Contracts.Documents;
using NetMetric.Search.Infrastructure.Meilisearch.Client;
using NetMetric.Search.Infrastructure.Meilisearch.Documents;
using NetMetric.Search.Infrastructure.Meilisearch.Indexing;
using NetMetric.Search.Infrastructure.Options;

namespace NetMetric.Search.Infrastructure.Meilisearch.Queries;

internal sealed class MeilisearchSearchQueryService(
    IOptions<SearchOptions> searchOptions,
    IMeilisearchIndexInitializer indexInitializer,
    IMeilisearchDocumentClient documentClient,
    MeilisearchFilterBuilder filterBuilder,
    MeilisearchDocumentMapper mapper,
    ILogger<MeilisearchSearchQueryService> logger) : ISearchQueryService
{
    public async Task<SearchResponse> SearchAsync(
        SearchRequest request,
        SearchAccessContext accessContext,
        CancellationToken cancellationToken)
    {
        if (!searchOptions.Value.Provider.Equals("Meilisearch", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"Search provider '{searchOptions.Value.Provider}' is not supported by {nameof(MeilisearchSearchQueryService)}.");
        }

        var plan = SearchFilterPlanBuilder.Build(request, accessContext);
        await indexInitializer.EnsureInitializedAsync(cancellationToken);

        var filter = filterBuilder.Build(plan);
        var providerRequest = new MeilisearchDocumentSearchRequest(
            IndexName: searchOptions.Value.IndexName,
            Query: plan.Query,
            Filter: filter,
            Page: plan.Page,
            PageSize: plan.PageSize,
            Sort: plan.Sort);

        var page = await documentClient.SearchAsync(providerRequest, cancellationToken);

        var visibleItems = new List<SearchResultItem>(page.Documents.Count);
        foreach (var storedDocument in page.Documents)
        {
            SearchDocument document;
            try
            {
                document = mapper.ToSearchDocument(storedDocument);
            }
            catch (Exception exception)
            {
                logger.LogWarning(exception, "Skipping malformed search document '{DocumentId}'.", storedDocument.Id);
                continue;
            }

            if (!SearchDocumentVisibilityEvaluator.CanAccess(document, accessContext))
            {
                continue;
            }

            visibleItems.Add(mapper.ToSearchResultItem(storedDocument));
        }

        // We return only post-filtered counts to avoid leaking hidden document counts.
        var safeTotalCount = visibleItems.Count;

        return new SearchResponse(
            Query: plan.Query,
            Page: plan.Page,
            PageSize: plan.PageSize,
            TotalCount: safeTotalCount,
            Items: visibleItems,
            PermissionPostFilteringApplied: plan.RequiresPermissionPostFiltering || plan.RequiresVisibilityPostFiltering);
    }
}
