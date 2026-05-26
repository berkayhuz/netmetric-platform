// <copyright file="ResolveIdentityCommandHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.CustomerIntelligence.Application.Abstractions.Persistence;
using NetMetric.CRM.CustomerIntelligence.Application.Features.Customer360;
using NetMetric.CRM.CustomerIntelligence.Contracts.DTOs;
using NetMetric.CRM.CustomerIntelligence.Domain.Entities.IdentityProfiles;
using NetMetric.CurrentUser;

namespace NetMetric.CRM.CustomerIntelligence.Application.Features.Cdp.Commands.ResolveIdentity;

public sealed class ResolveIdentityCommandHandler(ICustomerIntelligenceDbContext dbContext, ICurrentUserService currentUserService) : IRequestHandler<ResolveIdentityCommand, IdentityResolutionDto>
{
    public async Task<IdentityResolutionDto> Handle(ResolveIdentityCommand request, CancellationToken cancellationToken)
    {
        currentUserService.EnsureAuthenticated();

        var identityValue = request.IdentityValue.Trim();
        var entity = await dbContext.IdentityProfiles.FirstOrDefaultAsync(
            x => x.SubjectType == request.SubjectType &&
                 x.SubjectId == request.SubjectId &&
                 x.IdentityType == request.IdentityType &&
                 x.IdentityValue == identityValue,
            cancellationToken);
        var isNew = entity is null;

        entity ??= new IdentityProfile
        {
            TenantId = currentUserService.TenantId,
            SubjectType = request.SubjectType.Trim(),
            SubjectId = request.SubjectId,
            IdentityType = request.IdentityType.Trim(),
            IdentityValue = identityValue,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = currentUserService.UserName
        };

        entity.ConfidenceScore = request.ConfidenceScore;
        entity.ResolutionNotes = string.IsNullOrWhiteSpace(request.ResolutionNotes) ? null : request.ResolutionNotes.Trim();
        entity.LastResolvedAtUtc = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = currentUserService.UserName;

        if (isNew)
            await dbContext.IdentityProfiles.AddAsync(entity, cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);
        return entity.ToDto();
    }
}
