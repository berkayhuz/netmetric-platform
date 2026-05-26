// <copyright file="GetTagGroupsQueryHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.TagManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.TagManagement.Contracts.DTOs;

namespace NetMetric.CRM.TagManagement.Application.Features.TagGroups.Queries.GetTagGroups;

public sealed class GetTagGroupsQueryHandler(ITagManagementDbContext dbContext)
    : IRequestHandler<GetTagGroupsQuery, IReadOnlyList<TagGroupSummaryDto>>
{
    public async Task<IReadOnlyList<TagGroupSummaryDto>> Handle(
        GetTagGroupsQuery request,
        CancellationToken cancellationToken)
    {
        var groups = await dbContext.TagGroups
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => new
            {
                x.Id,
                x.Name,
                x.EntityType,
                x.DataJson
            })
            .ToListAsync(cancellationToken);

        return groups
            .Select(x => new TagGroupSummaryDto
            {
                Id = x.Id,
                Name = x.Name,
                EntityType = x.EntityType,
                Color = TryReadColor(x.DataJson)
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
