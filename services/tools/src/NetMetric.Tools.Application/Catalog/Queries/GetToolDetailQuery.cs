// <copyright file="GetToolDetailQuery.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.Tools.Application.Abstractions.Persistence;
using NetMetric.Tools.Contracts.Catalog;

namespace NetMetric.Tools.Application.Catalog.Queries;

public sealed record GetToolDetailQuery(string Slug) : IRequest<ToolDetailResponse?>;

public sealed class GetToolDetailQueryHandler(IToolsDbContext dbContext) : IRequestHandler<GetToolDetailQuery, ToolDetailResponse?>
{
    public async Task<ToolDetailResponse?> Handle(GetToolDetailQuery request, CancellationToken cancellationToken)
    {
        var slug = request.Slug.Trim().ToLowerInvariant();
        var tool = await dbContext.ToolDefinitions.AsNoTracking().FirstOrDefaultAsync(x => x.Slug == slug, cancellationToken);
        if (tool is null)
        {
            return null;
        }

        var categorySlug = await dbContext.ToolCategories
            .AsNoTracking()
            .Where(x => x.Id == tool.CategoryId)
            .Select(x => x.Slug)
            .FirstAsync(cancellationToken);

        return new ToolDetailResponse(
            tool.Slug,
            tool.Title,
            tool.Description,
            categorySlug,
            tool.ExecutionMode.ToString().ToLowerInvariant(),
            tool.AvailabilityStatus.ToString().ToLowerInvariant(),
            tool.IsEnabled,
            tool.AcceptedMimeTypesCsv.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries),
            tool.GuestMaxFileBytes,
            tool.AuthenticatedMaxSaveBytes,
            tool.SeoTitle,
            tool.SeoDescription);
    }
}
