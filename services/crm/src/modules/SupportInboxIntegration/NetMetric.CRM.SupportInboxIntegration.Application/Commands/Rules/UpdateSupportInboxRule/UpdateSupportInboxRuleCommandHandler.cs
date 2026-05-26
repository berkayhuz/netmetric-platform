// <copyright file="UpdateSupportInboxRuleCommandHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.SupportInboxIntegration.Application.Abstractions.Persistence;
using NetMetric.Exceptions;

namespace NetMetric.CRM.SupportInboxIntegration.Application.Commands.Rules.UpdateSupportInboxRule;

public sealed class UpdateSupportInboxRuleCommandHandler(ISupportInboxIntegrationDbContext dbContext) : IRequestHandler<UpdateSupportInboxRuleCommand>
{
    public async Task Handle(UpdateSupportInboxRuleCommand request, CancellationToken cancellationToken)
    {
        var entity = await dbContext.Rules.FirstOrDefaultAsync(x => x.Id == request.RuleId, cancellationToken)
            ?? throw new NotFoundAppException("Support inbox rule not found.");
        entity.Update(request.Name, request.MatchSender, request.MatchSubjectContains, request.AssignToQueueId, request.TicketCategoryId, request.SlaPolicyId, request.AutoCreateTicket, request.IsActive);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
