// <copyright file="CreateQuoteCommandHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.OpportunityManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.OpportunityManagement.Contracts.DTOs;
using NetMetric.CRM.QuoteManagement.Contracts.Integration;
using NetMetric.CurrentUser;
using NetMetric.Exceptions;

namespace NetMetric.CRM.OpportunityManagement.Application.Features.Quotes.Commands.CreateQuote;

public sealed class CreateQuoteCommandHandler(
    IOpportunityManagementDbContext dbContext,
    ICurrentUserService currentUserService,
    IQuoteCrossWriterService quoteCrossWriterService) : IRequestHandler<CreateQuoteCommand, QuoteDetailDto>
{
    public async Task<QuoteDetailDto> Handle(CreateQuoteCommand request, CancellationToken cancellationToken)
    {
        currentUserService.EnsureAuthenticated();

        var opportunity = await dbContext.Opportunities.FirstOrDefaultAsync(x => x.TenantId == currentUserService.TenantId && x.Id == request.OpportunityId, cancellationToken)
            ?? throw new NotFoundAppException("Opportunity not found.");

        var quote = await quoteCrossWriterService.CreateAsync(
            new QuoteCrossWriterCreateRequest(
                request.OpportunityId,
                opportunity.CustomerId,
                request.QuoteNumber,
                request.QuoteDate,
                request.ValidUntil,
                request.TermsAndConditions,
                request.OwnerUserId,
                request.CurrencyCode,
                request.ExchangeRate,
                request.Items.Select(x => new QuoteCrossWriterItemRequest(
                    x.ProductId,
                    x.Description,
                    x.Quantity,
                    x.UnitPrice,
                    x.DiscountRate,
                    x.TaxRate)).ToList()),
            cancellationToken);

        return new QuoteDetailDto(
            quote.Id,
            quote.QuoteNumber,
            quote.OpportunityId,
            quote.QuoteDate,
            quote.ValidUntil,
            quote.SubTotal,
            quote.DiscountTotal,
            quote.TaxTotal,
            quote.GrandTotal,
            quote.TermsAndConditions,
            quote.OwnerUserId,
            quote.CurrencyCode,
            quote.ExchangeRate,
            quote.Items.Select(x => new QuoteItemDto(
                x.Id,
                x.ProductId,
                x.Description,
                x.Quantity,
                x.UnitPrice,
                x.DiscountRate,
                x.TaxRate,
                x.LineTotal)).ToList(),
            quote.RowVersion);
    }
}
