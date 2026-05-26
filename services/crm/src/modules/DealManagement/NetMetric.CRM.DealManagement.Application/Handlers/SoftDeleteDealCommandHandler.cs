// <copyright file="SoftDeleteDealCommandHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.Core;
using NetMetric.CRM.DealManagement.Application.Abstractions.Integration;
using NetMetric.CRM.DealManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.DealManagement.Application.Commands.Deals;
using NetMetric.CRM.Sales;
using NetMetric.CurrentUser;

namespace NetMetric.CRM.DealManagement.Application.Handlers;

public sealed class SoftDeleteDealCommandHandler(
    IDealManagementDbContext dbContext,
    ICurrentUserService currentUserService,
    IDealManagementOutbox outbox) : IRequestHandler<SoftDeleteDealCommand>
{
    public async Task Handle(SoftDeleteDealCommand request, CancellationToken cancellationToken)
    {
        currentUserService.EnsureAuthenticated();
        var entity = await DealHandlerHelpers.RequireDealAsync(dbContext, request.DealId, cancellationToken);
        await AddDealTrashItemIfMissingAsync(entity, cancellationToken);
        entity.IsDeleted = true;
        entity.DeletedAt = DateTime.UtcNow;
        entity.DeletedBy = currentUserService.UserName;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = currentUserService.UserName;
        await outbox.EnqueueDealDeletedAsync(entity, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task AddDealTrashItemIfMissingAsync(Deal entity, CancellationToken cancellationToken)
    {
        var exists = await dbContext.GlobalTrashItems
            .AnyAsync(
                x => x.TenantId == entity.TenantId
                     && x.EntityType == CrmTrashEntityTypes.Deal
                     && x.EntityId == entity.Id
                     && x.Status == CrmTrashStatuses.Active,
                cancellationToken);

        if (exists)
        {
            return;
        }

        var deletedAtUtc = DateTime.UtcNow;
        var displayName = string.IsNullOrWhiteSpace(entity.Name) ? "Deleted deal" : entity.Name.Trim();
        var summary = string.IsNullOrWhiteSpace(entity.DealCode) ? null : entity.DealCode.Trim();

        await dbContext.GlobalTrashItems.AddAsync(
            new GlobalTrashItem
            {
                TenantId = entity.TenantId,
                EntityType = CrmTrashEntityTypes.Deal,
                EntityId = entity.Id,
                DisplayName = displayName,
                Summary = summary,
                SourceModule = "deals",
                OriginalRoute = $"/deals/{entity.Id:D}",
                DeletedAtUtc = deletedAtUtc,
                DeletedByUserId = currentUserService.UserId == Guid.Empty ? null : currentUserService.UserId,
                DeletedByDisplayName = currentUserService.UserName ?? currentUserService.Email,
                ExpiresAtUtc = deletedAtUtc.AddDays(7),
                Status = CrmTrashStatuses.Active
            },
            cancellationToken);
    }
}
