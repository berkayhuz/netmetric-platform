// <copyright file="GetTicketAssignmentHistoryQueryHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.TicketWorkflowManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.TicketWorkflowManagement.Application.DTOs;

namespace NetMetric.CRM.TicketWorkflowManagement.Application.Queries.GetTicketAssignmentHistory;

public sealed class GetTicketAssignmentHistoryQueryHandler : IRequestHandler<GetTicketAssignmentHistoryQuery, IReadOnlyList<TicketAssignmentHistoryDto>>
{
    private readonly ITicketWorkflowManagementDbContext _dbContext;

    public GetTicketAssignmentHistoryQueryHandler(ITicketWorkflowManagementDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<TicketAssignmentHistoryDto>> Handle(GetTicketAssignmentHistoryQuery request, CancellationToken cancellationToken)
    {
        return await _dbContext.TicketAssignmentHistories
            .Where(x => x.TicketId == request.TicketId)
            .OrderByDescending(x => x.ChangedAtUtc)
            .Select(x => new TicketAssignmentHistoryDto
            {
                Id = x.Id,
                TicketId = x.TicketId,
                PreviousOwnerUserId = x.PreviousOwnerUserId,
                NewOwnerUserId = x.NewOwnerUserId,
                PreviousQueueId = x.PreviousQueueId,
                NewQueueId = x.NewQueueId,
                ChangedByUserId = x.ChangedByUserId,
                Reason = x.Reason,
                ChangedAtUtc = x.ChangedAtUtc
            })
            .ToListAsync(cancellationToken);
    }
}
