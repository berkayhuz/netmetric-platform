// <copyright file="ScheduleIntegrationJobCommandHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System.Security.Cryptography;
using System.Text;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.IntegrationHub.Application.Abstractions.Persistence;
using NetMetric.CRM.IntegrationHub.Application.Security;
using NetMetric.CRM.IntegrationHub.Domain.Entities;
using NetMetric.Tenancy;

namespace NetMetric.CRM.IntegrationHub.Application.Commands.ScheduleIntegrationJob;

public sealed class ScheduleIntegrationJobCommandHandler(
    IIntegrationHubDbContext dbContext,
    ITenantContext tenantContext) : IRequestHandler<ScheduleIntegrationJobCommand, Guid>
{
    public async Task<Guid> Handle(ScheduleIntegrationJobCommand request, CancellationToken cancellationToken)
    {
        var tenantId = TenantRequestGuard.Resolve(tenantContext, request.TenantId);
        var providerKey = string.IsNullOrWhiteSpace(request.ProviderKey) ? request.JobType : request.ProviderKey.Trim();
        var idempotencyKey = string.IsNullOrWhiteSpace(request.IdempotencyKey)
            ? ComputeJobIdempotencyKey(tenantId, providerKey, request.JobType, request.Direction, request.PayloadJson)
            : request.IdempotencyKey.Trim();

        var existingJob = await dbContext.IntegrationJobs
            .FirstOrDefaultAsync(
                x => x.TenantId == tenantId &&
                     x.ProviderKey == providerKey &&
                     x.IdempotencyKey == idempotencyKey,
                cancellationToken);
        if (existingJob is not null)
        {
            return existingJob.Id;
        }

        var job = new IntegrationJob(
            tenantId,
            providerKey,
            request.JobType,
            request.Direction,
            request.PayloadJson,
            request.ScheduledAtUtc,
            idempotencyKey,
            request.MaxAttempts ?? 3,
            replayOfJobId: null);

        await dbContext.IntegrationJobs.AddAsync(job, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return job.Id;
    }

    private static string ComputeJobIdempotencyKey(Guid tenantId, string providerKey, string jobType, string direction, string payloadJson)
    {
        var normalizedPayload = string.IsNullOrWhiteSpace(payloadJson) ? "{}" : payloadJson.Trim();
        var material = $"{tenantId:N}|{providerKey.Trim()}|{jobType.Trim()}|{direction.Trim()}|{normalizedPayload}";
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(material));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
