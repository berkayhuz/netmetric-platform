// <copyright file="GetSmartLabelRulesQueryHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.TagManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.TagManagement.Contracts.DTOs;

namespace NetMetric.CRM.TagManagement.Application.Features.SmartLabels.Queries.GetSmartLabelRules;

public sealed class GetSmartLabelRulesQueryHandler(ITagManagementDbContext dbContext)
    : IRequestHandler<GetSmartLabelRulesQuery, IReadOnlyList<SmartLabelRuleSummaryDto>>
{
    public async Task<IReadOnlyList<SmartLabelRuleSummaryDto>> Handle(
        GetSmartLabelRulesQuery request,
        CancellationToken cancellationToken)
    {
        return await dbContext.SmartLabelRules
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => new SmartLabelRuleSummaryDto
            {
                Id = x.Id,
                Name = x.Name,
                EntityType = x.EntityType,
                ConditionJson = x.DataJson
            })
            .ToListAsync(cancellationToken);
    }
}
