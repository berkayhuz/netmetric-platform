// <copyright file="GetTenantSummaryQueryHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.TenantManagement.Application.Abstractions.Persistence;
namespace NetMetric.CRM.TenantManagement.Application.Queries.GetTenantSummary;

public sealed class GetTenantSummaryQueryHandler(ITenantManagementDbContext dbContext)
    : IRequestHandler<GetTenantSummaryQuery, TenantSummaryDto>
{
    public async Task<TenantSummaryDto> Handle(GetTenantSummaryQuery request, CancellationToken cancellationToken)
    {
        var tenant = await dbContext.TenantProfiles.FirstOrDefaultAsync(x => x.TenantId == request.TenantId, cancellationToken);

        if (tenant is null)
        {
            return new TenantSummaryDto(
                request.TenantId,
                "Workspace setup required",
                null,
                "tr-TR",
                "Europe/Istanbul",
                false,
                Array.Empty<TenantFlagDto>(),
                Array.Empty<TenantModuleDto>());
        }

        var flags = await dbContext.TenantFeatureFlags
            .Where(x => x.TenantId == request.TenantId)
            .Select(x => new TenantFlagDto(x.Key, x.IsEnabled, x.EffectiveFromUtc))
            .ToListAsync(cancellationToken);

        var modules = await dbContext.TenantModuleToggles
            .Where(x => x.TenantId == request.TenantId)
            .Select(x => new TenantModuleDto(x.ModuleKey, x.IsEnabled))
            .ToListAsync(cancellationToken);

        return new TenantSummaryDto(
            tenant.TenantId,
            tenant.Name,
            tenant.PrimaryDomain,
            tenant.Locale,
            tenant.TimeZone,
            tenant.IsProvisioned,
            flags,
            modules);
    }
}
