// <copyright file="GetToolsCatalogQuery.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.Tools.Application.Abstractions.Persistence;
using NetMetric.Tools.Contracts.Catalog;

namespace NetMetric.Tools.Application.Catalog.Queries;

public sealed record GetToolsCatalogQuery(string? Category = null, string? Search = null, bool PublicOnly = false) : IRequest<ToolCatalogResponse>;

public sealed class GetToolsCatalogQueryHandler(IToolsDbContext dbContext) : IRequestHandler<GetToolsCatalogQuery, ToolCatalogResponse>
{
    public async Task<ToolCatalogResponse> Handle(GetToolsCatalogQuery request, CancellationToken cancellationToken)
    {
        var categories = await dbContext.ToolCategories
            .AsNoTracking()
            .OrderBy(x => x.SortOrder)
            .Select(x => new ToolCategoryResponse(x.Slug, x.Title, x.Description, x.SortOrder))
            .ToListAsync(cancellationToken);

        var toolsQuery = dbContext.ToolDefinitions.AsNoTracking().AsQueryable();
        if (request.PublicOnly)
        {
            toolsQuery = toolsQuery.Where(x => x.IsEnabled);
        }

        if (!string.IsNullOrWhiteSpace(request.Category))
        {
            var normalizedCategory = request.Category.Trim().ToLowerInvariant();
            var categoryId = await dbContext.ToolCategories
                .Where(x => x.Slug == normalizedCategory)
                .Select(x => (Guid?)x.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (categoryId.HasValue)
            {
                toolsQuery = toolsQuery.Where(x => x.CategoryId == categoryId.Value);
            }
            else
            {
                toolsQuery = toolsQuery.Where(_ => false);
            }
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var q = request.Search.Trim().ToLowerInvariant();
            toolsQuery = toolsQuery.Where(x => x.Title.ToLower().Contains(q) || x.Description.ToLower().Contains(q));
        }

        var categoryLookup = await dbContext.ToolCategories.AsNoTracking().ToDictionaryAsync(x => x.Id, x => x.Slug, cancellationToken);

        var toolRows = await toolsQuery
            .OrderBy(x => x.Title)
            .ToListAsync(cancellationToken);

        var tools = toolRows
            .Select(x => new ToolCatalogItemResponse(
                x.Slug,
                x.Title,
                x.Description,
                categoryLookup[x.CategoryId],
                x.ExecutionMode.ToString().ToLowerInvariant(),
                x.AvailabilityStatus.ToString().ToLowerInvariant(),
                x.IsEnabled,
                x.AcceptedMimeTypesCsv.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries),
                x.GuestMaxFileBytes,
                x.AuthenticatedMaxSaveBytes,
                x.SeoTitle,
                x.SeoDescription))
            .ToList();

        return new ToolCatalogResponse(categories, tools);
    }
}
