// <copyright file="AcceptQuoteCommandHandler.cs" company="NetMetric">
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

public sealed class AcceptQuoteCommandHandler(
    IQuoteManagementDbContext dbContext,
    ICurrentUserService currentUserService,
    IQuoteManagementOutbox outbox) : QuoteWorkflowHandlerBase(dbContext, currentUserService, outbox), IRequestHandler<AcceptQuoteCommand>
{
    public async Task Handle(AcceptQuoteCommand request, CancellationToken cancellationToken)
    {
        var entity = await LoadAndCheckAsync(request.QuoteId, request.RowVersion, QuoteStateMachine.CanAccept, "Quote cannot be accepted from the current status.", cancellationToken);
        var oldStatus = entity.Status;
        entity.Status = QuoteStatusType.Accepted;
        entity.AcceptedAt = request.AcceptedAt ?? DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = CurrentUserService.UserName;
        await QuoteHandlerHelpers.AddHistoryAsync(DbContext, CurrentUserService, entity, oldStatus, entity.Status, request.Note ?? "Quote accepted by customer.", cancellationToken);
        await Outbox.EnqueueQuoteUpdatedAsync(entity, cancellationToken);
        await DbContext.SaveChangesAsync(cancellationToken);
    }
}
