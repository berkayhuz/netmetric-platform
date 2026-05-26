// <copyright file="DeletePipelineCommandHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.PipelineManagement.Application.Abstractions.Integration;
using NetMetric.CRM.PipelineManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.PipelineManagement.Application.Commands;
using NetMetric.CurrentUser;
using NetMetric.Exceptions;

namespace NetMetric.CRM.PipelineManagement.Application.Handlers;

public sealed class DeletePipelineCommandHandler(
    IPipelineManagementDbContext context,
    ICurrentUserService currentUserService,
    IPipelineManagementOutbox outbox)
    : IRequestHandler<DeletePipelineCommand>
{
    public async Task Handle(DeletePipelineCommand request, CancellationToken cancellationToken)
    {
        var tenantId = currentUserService.EnsureTenant();

        var pipeline = await context.Pipelines
            .FirstOrDefaultAsync(p => p.Id == request.Id && p.TenantId == tenantId, cancellationToken)
            ?? throw new NotFoundAppException("Pipeline not found.");

        // Check if any opportunities are using this pipeline
        var hasOpportunities = await context.Opportunities.AnyAsync(o => o.PipelineId == request.Id, cancellationToken);
        if (hasOpportunities)
            throw new ConflictAppException("Cannot delete pipeline because it has associated opportunities.");

        await outbox.EnqueuePipelineDeletedAsync(pipeline, cancellationToken);
        context.Pipelines.Remove(pipeline);
        await context.SaveChangesAsync(cancellationToken);
    }
}
