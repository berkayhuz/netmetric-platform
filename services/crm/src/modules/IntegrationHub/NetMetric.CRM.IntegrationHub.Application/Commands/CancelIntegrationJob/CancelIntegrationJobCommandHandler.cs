// <copyright file="CancelIntegrationJobCommandHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.IntegrationHub.Application.Abstractions.Persistence;
using NetMetric.CRM.IntegrationHub.Application.Security;
using NetMetric.CRM.IntegrationHub.Domain.Entities;
using NetMetric.Exceptions;
using NetMetric.Tenancy;

namespace NetMetric.CRM.IntegrationHub.Application.Commands.CancelIntegrationJob;

public sealed class CancelIntegrationJobCommandHandler(
    IIntegrationHubDbContext dbContext,
    ITenantContext tenantContext) : IRequestHandler<CancelIntegrationJobCommand>
{
    public async Task Handle(CancelIntegrationJobCommand request, CancellationToken cancellationToken)
    {
        var tenantId = TenantRequestGuard.Resolve(tenantContext, request.TenantId);
        var job = await dbContext.IntegrationJobs
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == request.JobId, cancellationToken)
            ?? throw new NotFoundAppException("Integration job not found.");

        var now = DateTime.UtcNow;
        job.Cancel(now, request.Reason);

        await dbContext.IntegrationJobExecutionLogs.AddAsync(
            new IntegrationJobExecutionLog(
                tenantId,
                job.Id,
                job.AttemptCount,
                job.Status,
                string.IsNullOrWhiteSpace(request.Reason) ? "Job cancellation requested." : $"Job cancellation requested: {request.Reason.Trim()}",
                now,
                now,
                job.CorrelationId ?? $"{tenantId:N}-{job.Id:N}-cancel"),
            cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
