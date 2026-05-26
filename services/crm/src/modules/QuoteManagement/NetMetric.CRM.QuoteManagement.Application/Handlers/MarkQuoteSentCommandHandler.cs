// <copyright file="MarkQuoteSentCommandHandler.cs" company="NetMetric">
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

public sealed class MarkQuoteSentCommandHandler(
    IQuoteManagementDbContext dbContext,
    ICurrentUserService currentUserService,
    IQuoteManagementOutbox outbox) : QuoteWorkflowHandlerBase(dbContext, currentUserService, outbox), IRequestHandler<MarkQuoteSentCommand>
{
    public async Task Handle(MarkQuoteSentCommand request, CancellationToken cancellationToken)
    {
        var entity = await LoadAndCheckAsync(request.QuoteId, request.RowVersion, QuoteStateMachine.CanSend, "Quote cannot be sent from the current status.", cancellationToken);
        var oldStatus = entity.Status;
        entity.Status = QuoteStatusType.Sent;
        entity.SentAt = request.SentAt ?? DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = CurrentUserService.UserName;
        await QuoteHandlerHelpers.AddHistoryAsync(DbContext, CurrentUserService, entity, oldStatus, entity.Status, request.Note ?? "Quote sent to customer.", cancellationToken);
        await Outbox.EnqueueQuoteUpdatedAsync(entity, cancellationToken);
        await DbContext.SaveChangesAsync(cancellationToken);
    }
}
