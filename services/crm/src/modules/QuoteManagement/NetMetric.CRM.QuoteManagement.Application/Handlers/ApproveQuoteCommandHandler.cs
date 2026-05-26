// <copyright file="ApproveQuoteCommandHandler.cs" company="NetMetric">
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

public sealed class ApproveQuoteCommandHandler(
    IQuoteManagementDbContext dbContext,
    ICurrentUserService currentUserService,
    IQuoteManagementOutbox outbox) : QuoteWorkflowHandlerBase(dbContext, currentUserService, outbox), IRequestHandler<ApproveQuoteCommand>
{
    public async Task Handle(ApproveQuoteCommand request, CancellationToken cancellationToken)
    {
        var entity = await LoadAndCheckAsync(request.QuoteId, request.RowVersion, QuoteStateMachine.CanApprove, "Quote cannot be approved from the current status.", cancellationToken);
        var oldStatus = entity.Status;
        entity.Status = QuoteStatusType.Approved;
        entity.ApprovedAt = DateTime.UtcNow;
        entity.ApprovedByUserId = CurrentUserService.UserId;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = CurrentUserService.UserName;
        await QuoteHandlerHelpers.AddHistoryAsync(DbContext, CurrentUserService, entity, oldStatus, entity.Status, request.Note ?? "Quote approved.", cancellationToken);
        await Outbox.EnqueueQuoteUpdatedAsync(entity, cancellationToken);
        await DbContext.SaveChangesAsync(cancellationToken);
    }
}
