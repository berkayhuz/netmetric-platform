// <copyright file="UpdateTicketQueueCommandHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.TicketWorkflowManagement.Application.Abstractions.Persistence;

namespace NetMetric.CRM.TicketWorkflowManagement.Application.Commands.Queues.UpdateTicketQueue;

public sealed class UpdateTicketQueueCommandHandler : IRequestHandler<UpdateTicketQueueCommand>
{
    private readonly ITicketWorkflowManagementDbContext _dbContext;

    public UpdateTicketQueueCommandHandler(ITicketWorkflowManagementDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Handle(UpdateTicketQueueCommand request, CancellationToken cancellationToken)
    {
        var queue = await _dbContext.TicketQueues.FirstOrDefaultAsync(x => x.Id == request.QueueId, cancellationToken)
            ?? throw new InvalidOperationException("Ticket queue not found.");

        queue.Update(request.Name, request.Description, request.AssignmentStrategy, request.IsDefault);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
