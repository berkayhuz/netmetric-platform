// <copyright file="CreateProposalTemplateCommandHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.QuoteManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.QuoteManagement.Application.Commands.ProposalTemplates;
using NetMetric.CRM.QuoteManagement.Application.Common;
using NetMetric.CRM.QuoteManagement.Contracts.DTOs;
using NetMetric.CRM.QuoteManagement.Domain.Entities;
using NetMetric.CurrentUser;

namespace NetMetric.CRM.QuoteManagement.Application.Handlers;

public sealed class CreateProposalTemplateCommandHandler(IQuoteManagementDbContext dbContext, ICurrentUserService currentUserService) : IRequestHandler<CreateProposalTemplateCommand, ProposalTemplateDto>
{
    public async Task<ProposalTemplateDto> Handle(CreateProposalTemplateCommand request, CancellationToken cancellationToken)
    {
        currentUserService.EnsureAuthenticated();
        if (request.IsDefault)
        {
            var defaults = await dbContext.ProposalTemplates.Where(x => x.IsDefault).ToListAsync(cancellationToken);
            defaults.ForEach(x => x.IsDefault = false);
        }

        var entity = new ProposalTemplate
        {
            TenantId = currentUserService.TenantId,
            Name = request.Name.Trim(),
            SubjectTemplate = string.IsNullOrWhiteSpace(request.SubjectTemplate) ? null : request.SubjectTemplate.Trim(),
            BodyTemplate = request.BodyTemplate.Trim(),
            IsDefault = request.IsDefault,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            CreatedBy = currentUserService.UserName,
            UpdatedBy = currentUserService.UserName
        };
        entity.SetNotes(request.Notes);

        await dbContext.ProposalTemplates.AddAsync(entity, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        return entity.ToDto();
    }
}
