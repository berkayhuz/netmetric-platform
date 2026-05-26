// <copyright file="GetSupportInboxConnectionsQueryHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.SupportInboxIntegration.Application.Abstractions.Persistence;
using NetMetric.CRM.SupportInboxIntegration.Contracts.DTOs;

namespace NetMetric.CRM.SupportInboxIntegration.Application.Queries.Connections.GetSupportInboxConnections;

public sealed class GetSupportInboxConnectionsQueryHandler(ISupportInboxIntegrationDbContext dbContext) : IRequestHandler<GetSupportInboxConnectionsQuery, IReadOnlyList<SupportInboxConnectionDto>>
{
    public async Task<IReadOnlyList<SupportInboxConnectionDto>> Handle(GetSupportInboxConnectionsQuery request, CancellationToken cancellationToken)
        => await dbContext.Connections.OrderBy(x => x.Name)
            .Select(x => new SupportInboxConnectionDto(x.Id, x.Name, x.Provider.ToString(), x.EmailAddress, x.Host, x.Port, x.Username, x.UseSsl, x.IsActive))
            .ToListAsync(cancellationToken);
}
