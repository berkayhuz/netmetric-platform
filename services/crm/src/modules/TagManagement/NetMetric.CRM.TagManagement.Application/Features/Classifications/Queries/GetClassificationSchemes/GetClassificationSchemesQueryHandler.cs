// <copyright file="GetClassificationSchemesQueryHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.TagManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.TagManagement.Contracts.DTOs;

namespace NetMetric.CRM.TagManagement.Application.Features.Classifications.Queries.GetClassificationSchemes;

public sealed class GetClassificationSchemesQueryHandler(ITagManagementDbContext dbContext)
    : IRequestHandler<GetClassificationSchemesQuery, IReadOnlyList<ClassificationSchemeSummaryDto>>
{
    public async Task<IReadOnlyList<ClassificationSchemeSummaryDto>> Handle(
        GetClassificationSchemesQuery request,
        CancellationToken cancellationToken)
    {
        return await dbContext.ClassificationSchemes
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => new ClassificationSchemeSummaryDto
            {
                Id = x.Id,
                Name = x.Name,
                EntityType = x.EntityType
            })
            .ToListAsync(cancellationToken);
    }
}
