// <copyright file="GetLeadConversionPreviewQueryHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.PipelineManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.PipelineManagement.Application.Queries;
using NetMetric.CRM.PipelineManagement.Contracts.DTOs;
using NetMetric.CurrentUser;
using NetMetric.Exceptions;

namespace NetMetric.CRM.PipelineManagement.Application.Handlers;

public sealed class GetLeadConversionPreviewQueryHandler(
    IPipelineManagementDbContext dbContext,
    ICurrentUserService currentUserService)
    : IRequestHandler<GetLeadConversionPreviewQuery, LeadConversionPreviewDto>
{
    public async Task<LeadConversionPreviewDto> Handle(GetLeadConversionPreviewQuery request, CancellationToken cancellationToken)
    {
        currentUserService.EnsureAuthenticated();
        var tenantId = currentUserService.EnsureTenant();

        var lead = await dbContext.Leads.FirstOrDefaultAsync(
                x => x.Id == request.LeadId && x.TenantId == tenantId,
                cancellationToken)
            ?? throw new NotFoundAppException("Lead not found.");

        return new LeadConversionPreviewDto(
            lead.Id,
            lead.LeadCode,
            lead.FullName,
            lead.CompanyName,
            lead.Email,
            lead.EstimatedBudget,
            lead.ConvertedCustomerId.HasValue,
            lead.ConvertedCustomerId);
    }
}
