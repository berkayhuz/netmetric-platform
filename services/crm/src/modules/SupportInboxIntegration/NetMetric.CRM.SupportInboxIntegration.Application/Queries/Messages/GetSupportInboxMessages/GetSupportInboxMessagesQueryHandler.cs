// <copyright file="GetSupportInboxMessagesQueryHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.SupportInboxIntegration.Application.Abstractions.Persistence;
using NetMetric.CRM.SupportInboxIntegration.Contracts.DTOs;
using NetMetric.Pagination;

namespace NetMetric.CRM.SupportInboxIntegration.Application.Queries.Messages.GetSupportInboxMessages;

public sealed class GetSupportInboxMessagesQueryHandler(ISupportInboxIntegrationDbContext dbContext) : IRequestHandler<GetSupportInboxMessagesQuery, PagedResult<SupportInboxMessageDto>>
{
    public async Task<PagedResult<SupportInboxMessageDto>> Handle(GetSupportInboxMessagesQuery request, CancellationToken cancellationToken)
    {
        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = Math.Clamp(request.PageSize <= 0 ? 20 : request.PageSize, 1, 200);
        var query = dbContext.Messages.AsQueryable();
        if (request.ConnectionId.HasValue)
            query = query.Where(x => x.ConnectionId == request.ConnectionId.Value);
        if (request.LinkedToTicket.HasValue)
            query = request.LinkedToTicket.Value ? query.Where(x => x.TicketId != null) : query.Where(x => x.TicketId == null);
        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query.OrderByDescending(x => x.ReceivedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new SupportInboxMessageDto(x.Id, x.ConnectionId, x.TicketId, x.ExternalMessageId, x.FromAddress, x.Subject, x.ReceivedAtUtc, x.ProcessingStatus.ToString(), x.ProcessingError))
            .ToListAsync(cancellationToken);
        return new PagedResult<SupportInboxMessageDto> { Items = items, TotalCount = totalCount, PageNumber = page, PageSize = pageSize };
    }
}
