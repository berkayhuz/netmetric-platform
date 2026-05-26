// <copyright file="SoftDeleteTicketQueueCommandHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.TicketWorkflowManagement.Application.Abstractions.Persistence;

namespace NetMetric.CRM.TicketWorkflowManagement.Application.Commands.Queues.SoftDeleteTicketQueue;

public sealed class SoftDeleteTicketQueueCommandHandler : IRequestHandler<SoftDeleteTicketQueueCommand>
{
    private readonly ITicketWorkflowManagementDbContext _dbContext;

    public SoftDeleteTicketQueueCommandHandler(ITicketWorkflowManagementDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Handle(SoftDeleteTicketQueueCommand request, CancellationToken cancellationToken)
    {
        var queue = await _dbContext.TicketQueues.FirstOrDefaultAsync(x => x.Id == request.QueueId, cancellationToken)
            ?? throw new InvalidOperationException("Ticket queue not found.");

        queue.IsDeleted = true;
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
