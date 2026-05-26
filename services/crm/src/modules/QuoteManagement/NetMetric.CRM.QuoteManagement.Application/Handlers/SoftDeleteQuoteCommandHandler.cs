// <copyright file="SoftDeleteQuoteCommandHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.Core;
using NetMetric.CRM.QuoteManagement.Application.Abstractions.Integration;
using NetMetric.CRM.QuoteManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.QuoteManagement.Application.Commands.Quotes;
using NetMetric.CRM.Sales;
using NetMetric.CurrentUser;

namespace NetMetric.CRM.QuoteManagement.Application.Handlers;

public sealed class SoftDeleteQuoteCommandHandler(
    IQuoteManagementDbContext dbContext,
    ICurrentUserService currentUserService,
    IQuoteManagementOutbox outbox) : IRequestHandler<SoftDeleteQuoteCommand>
{
    public async Task Handle(SoftDeleteQuoteCommand request, CancellationToken cancellationToken)
    {
        currentUserService.EnsureAuthenticated();
        var entity = await QuoteHandlerHelpers.RequireQuoteAsync(dbContext, request.QuoteId, cancellationToken);
        await AddQuoteTrashItemIfMissingAsync(entity, cancellationToken);
        entity.IsDeleted = true;
        entity.DeletedAt = DateTime.UtcNow;
        entity.DeletedBy = currentUserService.UserName;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = currentUserService.UserName;
        await outbox.EnqueueQuoteDeletedAsync(entity, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task AddQuoteTrashItemIfMissingAsync(Quote entity, CancellationToken cancellationToken)
    {
        var exists = await dbContext.GlobalTrashItems
            .AnyAsync(
                x => x.TenantId == entity.TenantId
                     && x.EntityType == CrmTrashEntityTypes.Quote
                     && x.EntityId == entity.Id
                     && x.Status == CrmTrashStatuses.Active,
                cancellationToken);

        if (exists)
        {
            return;
        }

        var deletedAtUtc = DateTime.UtcNow;
        var displayName = !string.IsNullOrWhiteSpace(entity.Title)
            ? entity.Title.Trim()
            : !string.IsNullOrWhiteSpace(entity.QuoteNumber)
                ? entity.QuoteNumber.Trim()
                : "Deleted quote";
        var summary = string.IsNullOrWhiteSpace(entity.QuoteNumber) ? null : entity.QuoteNumber.Trim();

        await dbContext.GlobalTrashItems.AddAsync(
            new GlobalTrashItem
            {
                TenantId = entity.TenantId,
                EntityType = CrmTrashEntityTypes.Quote,
                EntityId = entity.Id,
                DisplayName = displayName,
                Summary = summary,
                SourceModule = "quotes",
                OriginalRoute = $"/quotes/{entity.Id:D}",
                DeletedAtUtc = deletedAtUtc,
                DeletedByUserId = currentUserService.UserId == Guid.Empty ? null : currentUserService.UserId,
                DeletedByDisplayName = currentUserService.UserName ?? currentUserService.Email,
                ExpiresAtUtc = deletedAtUtc.AddDays(7),
                Status = CrmTrashStatuses.Active
            },
            cancellationToken);
    }
}
