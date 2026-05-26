// <copyright file="GetProposalTemplatesQueryHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.QuoteManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.QuoteManagement.Application.Common;
using NetMetric.CRM.QuoteManagement.Application.Queries.ProposalTemplates;
using NetMetric.CRM.QuoteManagement.Contracts.DTOs;

namespace NetMetric.CRM.QuoteManagement.Application.Handlers;

public sealed class GetProposalTemplatesQueryHandler(IQuoteManagementDbContext dbContext) : IRequestHandler<GetProposalTemplatesQuery, IReadOnlyList<ProposalTemplateDto>>
{
    public async Task<IReadOnlyList<ProposalTemplateDto>> Handle(GetProposalTemplatesQuery request, CancellationToken cancellationToken)
    {
        var query = dbContext.ProposalTemplates.AsNoTracking().AsQueryable();
        if (request.IsActive.HasValue)
            query = query.Where(x => x.IsActive == request.IsActive.Value);
        var items = await query.OrderByDescending(x => x.IsDefault).ThenBy(x => x.Name).ToListAsync(cancellationToken);
        return items.Select(x => x.ToDto()).ToList();
    }
}
