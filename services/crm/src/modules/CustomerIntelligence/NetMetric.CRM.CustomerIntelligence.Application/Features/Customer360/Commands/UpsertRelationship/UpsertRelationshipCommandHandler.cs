// <copyright file="UpsertRelationshipCommandHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.CustomerIntelligence.Application.Abstractions.Persistence;
using NetMetric.CRM.CustomerIntelligence.Contracts.DTOs;
using NetMetric.CRM.CustomerIntelligence.Domain.Entities.CustomerRelationships;
using NetMetric.CurrentUser;

namespace NetMetric.CRM.CustomerIntelligence.Application.Features.Customer360.Commands.UpsertRelationship;

public sealed class UpsertRelationshipCommandHandler(ICustomerIntelligenceDbContext dbContext, ICurrentUserService currentUserService) : IRequestHandler<UpsertRelationshipCommand, RelationshipEdgeDto>
{
    public async Task<RelationshipEdgeDto> Handle(UpsertRelationshipCommand request, CancellationToken cancellationToken)
    {
        currentUserService.EnsureAuthenticated();

        var entity = await dbContext.CustomerRelationships.FirstOrDefaultAsync(
            x => x.SourceEntityType == request.SourceEntityType &&
                 x.SourceEntityId == request.SourceEntityId &&
                 x.TargetEntityType == request.TargetEntityType &&
                 x.TargetEntityId == request.TargetEntityId &&
                 x.RelationshipType == request.RelationshipType,
            cancellationToken);
        var isNew = entity is null;

        entity ??= CustomerRelationship.Create(request.Name, request.TargetEntityType, request.TargetEntityId, request.DataJson);
        entity.TenantId = currentUserService.TenantId;
        entity.SourceEntityType = request.SourceEntityType.Trim();
        entity.SourceEntityId = request.SourceEntityId;
        entity.TargetEntityType = request.TargetEntityType.Trim();
        entity.TargetEntityId = request.TargetEntityId;
        entity.Name = request.Name.Trim();
        entity.RelationshipType = request.RelationshipType.Trim();
        entity.StrengthScore = request.StrengthScore;
        entity.IsBidirectional = request.IsBidirectional;
        entity.EntityType = request.TargetEntityType.Trim();
        entity.RelatedEntityId = request.TargetEntityId;
        entity.DataJson = string.IsNullOrWhiteSpace(request.DataJson) ? null : request.DataJson.Trim();
        entity.CreatedAt = entity.CreatedAt == default ? DateTime.UtcNow : entity.CreatedAt;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.CreatedBy ??= currentUserService.UserName;
        entity.UpdatedBy = currentUserService.UserName;

        if (isNew)
            await dbContext.CustomerRelationships.AddAsync(entity, cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);
        return entity.ToDto();
    }
}
