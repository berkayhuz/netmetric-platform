// <copyright file="GetQuoteWorkspaceQueryHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.Authorization;
using NetMetric.CRM.Authorization;
using NetMetric.CRM.QuoteManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.QuoteManagement.Application.Common;
using NetMetric.CRM.QuoteManagement.Application.Queries.Quotes;
using NetMetric.CRM.QuoteManagement.Contracts.DTOs;

namespace NetMetric.CRM.QuoteManagement.Application.Handlers;

public sealed class GetQuoteWorkspaceQueryHandler(
    IQuoteManagementDbContext dbContext,
    ICurrentAuthorizationScope authorizationScope,
    IFieldAuthorizationService fieldAuthorizationService) : IRequestHandler<GetQuoteWorkspaceQuery, QuoteWorkspaceDto?>
{
    public async Task<QuoteWorkspaceDto?> Handle(GetQuoteWorkspaceQuery request, CancellationToken cancellationToken)
    {
        var scope = authorizationScope.Resolve(CrmAuthorizationCatalog.QuotesResource);
        var canSeeFinancialData = fieldAuthorizationService.Decide(scope.Resource, "financialData", scope.Permissions).Visibility >= FieldVisibility.Visible;
        var canSeeInternalNotes = fieldAuthorizationService.Decide(scope.Resource, "notes", scope.Permissions).Visibility >= FieldVisibility.Visible;
        var quote = await dbContext.Quotes.Include(x => x.Items).AsNoTracking()
            .ApplyRowScope(scope, x => x.TenantId, x => x.OwnerUserId, x => x.OwnerUserId)
            .FirstOrDefaultAsync(x => x.Id == request.QuoteId, cancellationToken);
        if (quote is null)
            return null;
        var history = await QuoteHandlerHelpers.LoadHistoryAsync(dbContext, quote.Id, cancellationToken);
        return quote.ToWorkspaceDto(history, canSeeFinancialData, canSeeInternalNotes);
    }
}
