// <copyright file="QuoteWorkflowHandlerBase.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.CRM.QuoteManagement.Application.Abstractions.Integration;
using NetMetric.CRM.QuoteManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.Sales;
using NetMetric.CRM.Types;
using NetMetric.CurrentUser;
using NetMetric.Exceptions;

namespace NetMetric.CRM.QuoteManagement.Application.Handlers;

public abstract class QuoteWorkflowHandlerBase(
    IQuoteManagementDbContext dbContext,
    ICurrentUserService currentUserService,
    IQuoteManagementOutbox outbox)
{
    protected IQuoteManagementDbContext DbContext { get; } = dbContext;
    protected ICurrentUserService CurrentUserService { get; } = currentUserService;
    protected IQuoteManagementOutbox Outbox { get; } = outbox;

    protected async Task<Quote> LoadAndCheckAsync(Guid quoteId, string? rowVersion, Func<QuoteStatusType, bool> predicate, string message, CancellationToken cancellationToken)
    {
        CurrentUserService.EnsureAuthenticated();
        var entity = await QuoteHandlerHelpers.RequireQuoteAsync(DbContext, quoteId, cancellationToken);
        if (!predicate(entity.Status))
            throw new ConflictAppException(message);
        QuoteHandlerHelpers.ApplyRowVersion(DbContext, entity, rowVersion);
        return entity;
    }
}
