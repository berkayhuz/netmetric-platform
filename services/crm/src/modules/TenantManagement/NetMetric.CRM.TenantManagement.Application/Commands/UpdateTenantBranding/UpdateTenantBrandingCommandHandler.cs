// <copyright file="UpdateTenantBrandingCommandHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.TenantManagement.Application.Abstractions.Persistence;

namespace NetMetric.CRM.TenantManagement.Application.Commands.UpdateTenantBranding;

public sealed class UpdateTenantBrandingCommandHandler(ITenantManagementDbContext dbContext)
    : IRequestHandler<UpdateTenantBrandingCommand>
{
    public async Task Handle(UpdateTenantBrandingCommand request, CancellationToken cancellationToken)
    {
        var tenant = await dbContext.TenantProfiles.FirstAsync(x => x.TenantId == request.TenantId, cancellationToken);
        tenant.UpdateBranding(request.PrimaryDomain, request.Locale, request.TimeZone, request.BrandPrimaryColor, request.LogoUrl);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
