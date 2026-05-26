// <copyright file="ProvisionTenantCommandHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.TenantManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.TenantManagement.Domain.Entities;

namespace NetMetric.CRM.TenantManagement.Application.Commands.ProvisionTenant;

public sealed class ProvisionTenantCommandHandler(ITenantManagementDbContext dbContext)
    : IRequestHandler<ProvisionTenantCommand, Guid>
{
    public async Task<Guid> Handle(ProvisionTenantCommand request, CancellationToken cancellationToken)
    {
        var existing = await dbContext.TenantProfiles.AnyAsync(x => x.TenantId == request.TenantId, cancellationToken);
        if (existing)
            return request.TenantId;

        var tenant = new TenantProfile(request.TenantId, request.Name);
        tenant.MarkProvisioned();

        await dbContext.TenantProfiles.AddAsync(tenant, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return request.TenantId;
    }
}
