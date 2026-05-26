// <copyright file="DeleteProposalTemplateCommandHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.QuoteManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.QuoteManagement.Application.Commands.ProposalTemplates;
using NetMetric.CurrentUser;
using NetMetric.Exceptions;

namespace NetMetric.CRM.QuoteManagement.Application.Handlers;

public sealed class DeleteProposalTemplateCommandHandler(IQuoteManagementDbContext dbContext, ICurrentUserService currentUserService) : IRequestHandler<DeleteProposalTemplateCommand>
{
    public async Task Handle(DeleteProposalTemplateCommand request, CancellationToken cancellationToken)
    {
        currentUserService.EnsureAuthenticated();
        var entity = await dbContext.ProposalTemplates.FirstOrDefaultAsync(x => x.Id == request.TemplateId, cancellationToken)
                     ?? throw new NotFoundAppException("Proposal template not found.");
        entity.IsDeleted = true;
        entity.DeletedAt = DateTime.UtcNow;
        entity.DeletedBy = currentUserService.UserName;
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
