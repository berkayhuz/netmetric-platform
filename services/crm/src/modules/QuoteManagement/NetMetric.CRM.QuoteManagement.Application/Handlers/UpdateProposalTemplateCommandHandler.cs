// <copyright file="UpdateProposalTemplateCommandHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.QuoteManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.QuoteManagement.Application.Commands.ProposalTemplates;
using NetMetric.CRM.QuoteManagement.Application.Common;
using NetMetric.CRM.QuoteManagement.Contracts.DTOs;
using NetMetric.CurrentUser;
using NetMetric.Exceptions;

namespace NetMetric.CRM.QuoteManagement.Application.Handlers;

public sealed class UpdateProposalTemplateCommandHandler(IQuoteManagementDbContext dbContext, ICurrentUserService currentUserService) : IRequestHandler<UpdateProposalTemplateCommand, ProposalTemplateDto>
{
    public async Task<ProposalTemplateDto> Handle(UpdateProposalTemplateCommand request, CancellationToken cancellationToken)
    {
        currentUserService.EnsureAuthenticated();
        var entity = await dbContext.ProposalTemplates.FirstOrDefaultAsync(x => x.Id == request.TemplateId, cancellationToken)
                     ?? throw new NotFoundAppException("Proposal template not found.");

        if (request.IsDefault)
        {
            var defaults = await dbContext.ProposalTemplates.Where(x => x.IsDefault && x.Id != entity.Id).ToListAsync(cancellationToken);
            defaults.ForEach(x => x.IsDefault = false);
        }

        entity.Name = request.Name.Trim();
        entity.SubjectTemplate = string.IsNullOrWhiteSpace(request.SubjectTemplate) ? null : request.SubjectTemplate.Trim();
        entity.BodyTemplate = request.BodyTemplate.Trim();
        entity.IsDefault = request.IsDefault;
        entity.SetNotes(request.Notes);
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = currentUserService.UserName;

        await dbContext.SaveChangesAsync(cancellationToken);
        return entity.ToDto();
    }
}
