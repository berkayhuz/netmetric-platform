// <copyright file="GetCpqWorkspaceQueryHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.QuoteManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.QuoteManagement.Application.Common;
using NetMetric.CRM.QuoteManagement.Application.Queries.Quotes;
using NetMetric.CRM.QuoteManagement.Contracts.DTOs;

namespace NetMetric.CRM.QuoteManagement.Application.Handlers;

public sealed class GetCpqWorkspaceQueryHandler(IQuoteManagementDbContext dbContext) : IRequestHandler<GetCpqWorkspaceQuery, CpqWorkspaceDto>
{
    public async Task<CpqWorkspaceDto> Handle(GetCpqWorkspaceQuery request, CancellationToken cancellationToken)
    {
        var rules = await dbContext.ProductRules.AsNoTracking().OrderBy(x => x.Name).ToListAsync(cancellationToken);
        var bundles = await dbContext.ProductBundles.Include(x => x.Items).AsNoTracking().OrderBy(x => x.Name).ToListAsync(cancellationToken);
        var playbooks = await dbContext.GuidedSellingPlaybooks.AsNoTracking().OrderBy(x => x.Name).ToListAsync(cancellationToken);

        return new CpqWorkspaceDto(
            rules.Select(x => x.ToDto()).ToList(),
            bundles.Select(x => x.ToDto()).ToList(),
            playbooks.Select(x => x.ToDto()).ToList());
    }
}
