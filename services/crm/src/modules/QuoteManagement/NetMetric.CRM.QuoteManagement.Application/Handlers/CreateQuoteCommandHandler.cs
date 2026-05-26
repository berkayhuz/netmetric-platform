// <copyright file="CreateQuoteCommandHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.QuoteManagement.Application.Abstractions.Integration;
using NetMetric.CRM.QuoteManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.QuoteManagement.Application.Abstractions.Services;
using NetMetric.CRM.QuoteManagement.Application.Commands.Quotes;
using NetMetric.CRM.QuoteManagement.Application.Common;
using NetMetric.CRM.QuoteManagement.Contracts.DTOs;
using NetMetric.CRM.Sales;
using NetMetric.CRM.Types;
using NetMetric.CurrentUser;
using NetMetric.Exceptions;

namespace NetMetric.CRM.QuoteManagement.Application.Handlers;

public sealed class CreateQuoteCommandHandler(
    IQuoteManagementDbContext dbContext,
    IQuoteProductReadModelSyncService quoteProductReadModelSyncService,
    IQuoteOpportunityReadModelSyncService quoteOpportunityReadModelSyncService,
    IQuoteCustomerReadModelSyncService quoteCustomerReadModelSyncService,
    ICurrentUserService currentUserService,
    IQuoteManagementOutbox outbox) : IRequestHandler<CreateQuoteCommand, QuoteDetailDto>
{
    public async Task<QuoteDetailDto> Handle(CreateQuoteCommand request, CancellationToken cancellationToken)
    {
        currentUserService.EnsureAuthenticated();
        Guid? resolvedCustomerId = request.CustomerId;

        if (request.OpportunityId.HasValue)
        {
            var opportunity = await quoteOpportunityReadModelSyncService.SyncAsync(
                request.OpportunityId.Value,
                cancellationToken);

            if (request.CustomerId.HasValue &&
                opportunity.CustomerId.HasValue &&
                request.CustomerId.Value != opportunity.CustomerId.Value)
            {
                throw new ValidationAppException(
                    "Selected customer does not match opportunity customer.",
                    new Dictionary<string, string[]>
                    {
                        ["customerId"] = ["Selected customer does not match opportunity customer."]
                    });
            }

            resolvedCustomerId ??= opportunity.CustomerId;
        }

        if (resolvedCustomerId.HasValue)
        {
            await quoteCustomerReadModelSyncService.SyncAsync(resolvedCustomerId.Value, cancellationToken);
        }

        var entity = new Quote
        {
            TenantId = currentUserService.TenantId,
            QuoteNumber = request.QuoteNumber.Trim(),
            ProposalTitle = string.IsNullOrWhiteSpace(request.ProposalTitle) ? null : request.ProposalTitle.Trim(),
            ProposalSummary = string.IsNullOrWhiteSpace(request.ProposalSummary) ? null : request.ProposalSummary.Trim(),
            ProposalBody = string.IsNullOrWhiteSpace(request.ProposalBody) ? null : request.ProposalBody.Trim(),
            QuoteDate = request.QuoteDate,
            ValidUntil = request.ValidUntil,
            OpportunityId = request.OpportunityId,
            CustomerId = resolvedCustomerId,
            OwnerUserId = request.OwnerUserId,
            CurrencyCode = request.CurrencyCode.Trim().ToUpperInvariant(),
            ExchangeRate = request.ExchangeRate,
            TermsAndConditions = string.IsNullOrWhiteSpace(request.TermsAndConditions) ? null : request.TermsAndConditions.Trim(),
            ProposalTemplateId = request.ProposalTemplateId,
            Status = QuoteStatusType.Draft,
            RevisionNumber = 1,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            CreatedBy = currentUserService.UserName,
            UpdatedBy = currentUserService.UserName
        };

        await quoteProductReadModelSyncService.SyncAsync(request.Items.Select(x => x.ProductId).ToArray(), cancellationToken);
        await QuoteHandlerHelpers.ValidateProductsAsync(dbContext, request.Items, cancellationToken);
        QuoteHandlerHelpers.Recalculate(entity, request.Items);
        await dbContext.Quotes.AddAsync(entity, cancellationToken);
        await outbox.EnqueueQuoteCreatedAsync(entity, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        await QuoteHandlerHelpers.AddHistoryAsync(dbContext, currentUserService, entity, null, QuoteStatusType.Draft, "Quote created.", cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        var history = await QuoteHandlerHelpers.LoadHistoryAsync(dbContext, entity.Id, cancellationToken);
        return entity.ToDetailDto(history);
    }
}
