// <copyright file="UpdateQuoteCommandHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.QuoteManagement.Application.Abstractions.Integration;
using NetMetric.CRM.QuoteManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.QuoteManagement.Application.Abstractions.Services;
using NetMetric.CRM.QuoteManagement.Application.Commands.Quotes;
using NetMetric.CRM.QuoteManagement.Application.Common;
using NetMetric.CRM.QuoteManagement.Contracts.DTOs;
using NetMetric.CurrentUser;
using NetMetric.Exceptions;

namespace NetMetric.CRM.QuoteManagement.Application.Handlers;

public sealed class UpdateQuoteCommandHandler(
    IQuoteManagementDbContext dbContext,
    IQuoteProductReadModelSyncService quoteProductReadModelSyncService,
    ICurrentUserService currentUserService,
    IQuoteManagementOutbox outbox) : IRequestHandler<UpdateQuoteCommand, QuoteDetailDto>
{
    public async Task<QuoteDetailDto> Handle(UpdateQuoteCommand request, CancellationToken cancellationToken)
    {
        currentUserService.EnsureAuthenticated();
        var entity = await QuoteHandlerHelpers.RequireQuoteAsync(dbContext, request.QuoteId, cancellationToken);

        if (!QuoteStateMachine.CanEdit(entity.Status))
            throw new ConflictAppException("Quote can only be edited in Draft or Rejected status.");

        QuoteHandlerHelpers.ApplyRowVersion(dbContext, entity, request.RowVersion);
        entity.QuoteNumber = request.QuoteNumber.Trim();
        entity.ProposalTitle = string.IsNullOrWhiteSpace(request.ProposalTitle) ? null : request.ProposalTitle.Trim();
        entity.ProposalSummary = string.IsNullOrWhiteSpace(request.ProposalSummary) ? null : request.ProposalSummary.Trim();
        entity.ProposalBody = string.IsNullOrWhiteSpace(request.ProposalBody) ? null : request.ProposalBody.Trim();
        entity.QuoteDate = request.QuoteDate;
        entity.ValidUntil = request.ValidUntil;
        entity.OpportunityId = request.OpportunityId;
        entity.CustomerId = request.CustomerId;
        entity.OwnerUserId = request.OwnerUserId;
        entity.CurrencyCode = request.CurrencyCode.Trim().ToUpperInvariant();
        entity.ExchangeRate = request.ExchangeRate;
        entity.TermsAndConditions = string.IsNullOrWhiteSpace(request.TermsAndConditions) ? null : request.TermsAndConditions.Trim();
        entity.ProposalTemplateId = request.ProposalTemplateId;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = currentUserService.UserName;
        await quoteProductReadModelSyncService.SyncAsync(request.Items.Select(x => x.ProductId).ToArray(), cancellationToken);
        await QuoteHandlerHelpers.ValidateProductsAsync(dbContext, request.Items, cancellationToken);
        var existingItems = entity.Items.ToList();
        await dbContext.QuoteItems.Where(x => x.QuoteId == entity.Id).ExecuteDeleteAsync(cancellationToken);

        if (dbContext is DbContext efDbContext)
        {
            foreach (var existingItem in existingItems)
                efDbContext.Entry(existingItem).State = EntityState.Detached;
        }

        entity.Items.Clear();
        var rebuiltItems = QuoteHandlerHelpers.BuildItems(entity, request.Items);
        QuoteHandlerHelpers.ApplyTotals(entity, request.Items);
        await dbContext.QuoteItems.AddRangeAsync(rebuiltItems, cancellationToken);
        await outbox.EnqueueQuoteUpdatedAsync(entity, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        var history = await QuoteHandlerHelpers.LoadHistoryAsync(dbContext, entity.Id, cancellationToken);
        return entity.ToDetailDto(history);
    }
}
