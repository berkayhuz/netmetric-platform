// <copyright file="GetTicketQueuesQueryHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.TicketWorkflowManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.TicketWorkflowManagement.Application.DTOs;

namespace NetMetric.CRM.TicketWorkflowManagement.Application.Queries.GetTicketQueues;

public sealed class GetTicketQueuesQueryHandler : IRequestHandler<GetTicketQueuesQuery, IReadOnlyList<TicketQueueDto>>
{
    private readonly ITicketWorkflowManagementDbContext _dbContext;

    public GetTicketQueuesQueryHandler(ITicketWorkflowManagementDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<TicketQueueDto>> Handle(GetTicketQueuesQuery request, CancellationToken cancellationToken)
    {
        return await _dbContext.TicketQueues
            .OrderBy(x => x.Name)
            .Select(x => new TicketQueueDto
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name,
                Description = x.Description,
                AssignmentStrategy = x.AssignmentStrategy.ToString(),
                IsDefault = x.IsDefault
            })
            .ToListAsync(cancellationToken);
    }
}
