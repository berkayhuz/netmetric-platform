// <copyright file="GetTagsQueryHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.TagManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.TagManagement.Contracts.DTOs;

namespace NetMetric.CRM.TagManagement.Application.Features.Tags.Queries.GetTags;

public sealed class GetTagsQueryHandler(ITagManagementDbContext dbContext)
    : IRequestHandler<GetTagsQuery, IReadOnlyList<TagSummaryDto>>
{
    public async Task<IReadOnlyList<TagSummaryDto>> Handle(
        GetTagsQuery request,
        CancellationToken cancellationToken)
    {
        var tags = await dbContext.TagDefinitions
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => new
            {
                x.Id,
                x.Name,
                x.DataJson
            })
            .ToListAsync(cancellationToken);

        return tags
            .Select(x => new TagSummaryDto
            {
                TagId = x.Id,
                Name = x.Name,
                Color = TryReadColor(x.DataJson) ?? "#64748b",
                GroupName = null
            })
            .ToList();
    }

    private static string? TryReadColor(string? dataJson)
    {
        if (string.IsNullOrWhiteSpace(dataJson))
        {
            return null;
        }

        try
        {
            using var document = JsonDocument.Parse(dataJson);
            return document.RootElement.TryGetProperty("color", out var color) ||
                document.RootElement.TryGetProperty("Color", out color)
                ? color.GetString()
                : null;
        }
        catch (JsonException)
        {
            return null;
        }
    }
}
