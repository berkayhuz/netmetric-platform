// <copyright file="CreateRevisionCommandHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.QuoteManagement.Application.Abstractions.Integration;
using NetMetric.CRM.QuoteManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.QuoteManagement.Application.Commands.Quotes;
using NetMetric.CRM.QuoteManagement.Application.Common;
using NetMetric.CRM.QuoteManagement.Contracts.DTOs;
using NetMetric.CRM.Sales;
using NetMetric.CRM.Types;
using NetMetric.CurrentUser;

namespace NetMetric.CRM.QuoteManagement.Application.Handlers;

public sealed class CreateRevisionCommandHandler(
    IQuoteManagementDbContext dbContext,
    ICurrentUserService currentUserService,
    IQuoteManagementOutbox outbox) : IRequestHandler<CreateRevisionCommand, QuoteDetailDto>
{
    public async Task<QuoteDetailDto> Handle(CreateRevisionCommand request, CancellationToken cancellationToken)
    {
        currentUserService.EnsureAuthenticated();
        var source = await QuoteHandlerHelpers.RequireQuoteAsync(dbContext, request.QuoteId, cancellationToken);
        var copy = new Quote
        {
            TenantId = currentUserService.TenantId,
            QuoteNumber = request.NewQuoteNumber.Trim(),
            ProposalTitle = source.ProposalTitle,
            ProposalSummary = source.ProposalSummary,
            ProposalBody = source.ProposalBody,
            QuoteDate = DateTime.UtcNow,
            ValidUntil = source.ValidUntil,
            SubTotal = source.SubTotal,
            DiscountTotal = source.DiscountTotal,
            TaxTotal = source.TaxTotal,
            GrandTotal = source.GrandTotal,
            TermsAndConditions = source.TermsAndConditions,
            OpportunityId = source.OpportunityId,
            CustomerId = source.CustomerId,
            OwnerUserId = source.OwnerUserId,
            CurrencyCode = source.CurrencyCode,
            ExchangeRate = source.ExchangeRate,
            Status = QuoteStatusType.Draft,
            ProposalTemplateId = source.ProposalTemplateId,
            RevisionNumber = source.RevisionNumber + 1,
            ParentQuoteId = source.Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            CreatedBy = currentUserService.UserName,
            UpdatedBy = currentUserService.UserName
        };

        foreach (var item in source.Items)
        {
            copy.Items.Add(new QuoteItem
            {
                TenantId = currentUserService.TenantId,
                ProductId = item.ProductId,
                Description = item.Description,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                DiscountRate = item.DiscountRate,
                TaxRate = item.TaxRate,
                LineTotal = item.LineTotal
            });
        }

        await dbContext.Quotes.AddAsync(copy, cancellationToken);
        await outbox.EnqueueQuoteCreatedAsync(copy, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        await QuoteHandlerHelpers.AddHistoryAsync(dbContext, currentUserService, copy, null, QuoteStatusType.Draft, $"Revision {copy.RevisionNumber} created from {source.QuoteNumber}.", cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        var history = await QuoteHandlerHelpers.LoadHistoryAsync(dbContext, copy.Id, cancellationToken);
        return copy.ToDetailDto(history);
    }
}
