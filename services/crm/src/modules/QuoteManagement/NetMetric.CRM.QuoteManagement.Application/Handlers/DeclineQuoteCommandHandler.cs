// <copyright file="DeclineQuoteCommandHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using NetMetric.CRM.QuoteManagement.Application.Abstractions.Integration;
using NetMetric.CRM.QuoteManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.QuoteManagement.Application.Commands.Quotes;
using NetMetric.CRM.QuoteManagement.Application.Common;
using NetMetric.CRM.Types;
using NetMetric.CurrentUser;

namespace NetMetric.CRM.QuoteManagement.Application.Handlers;

public sealed class DeclineQuoteCommandHandler(
    IQuoteManagementDbContext dbContext,
    ICurrentUserService currentUserService,
    IQuoteManagementOutbox outbox) : QuoteWorkflowHandlerBase(dbContext, currentUserService, outbox), IRequestHandler<DeclineQuoteCommand>
{
    public async Task Handle(DeclineQuoteCommand request, CancellationToken cancellationToken)
    {
        var entity = await LoadAndCheckAsync(request.QuoteId, request.RowVersion, QuoteStateMachine.CanDecline, "Quote cannot be declined from the current status.", cancellationToken);
        var oldStatus = entity.Status;
        entity.Status = QuoteStatusType.Declined;
        entity.DeclinedAt = request.DeclinedAt ?? DateTime.UtcNow;
        entity.DeclineReason = request.Reason.Trim();
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = CurrentUserService.UserName;
        await QuoteHandlerHelpers.AddHistoryAsync(DbContext, CurrentUserService, entity, oldStatus, entity.Status, request.Reason, cancellationToken);
        await Outbox.EnqueueQuoteUpdatedAsync(entity, cancellationToken);
        await DbContext.SaveChangesAsync(cancellationToken);
    }
}
