// <copyright file="ConvertLeadCommandHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.Core;
using NetMetric.CRM.OpportunityManagement.Contracts.Integration;
using NetMetric.CRM.PipelineManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.PipelineManagement.Application.Commands;
using NetMetric.CRM.PipelineManagement.Application.Common;
using NetMetric.CRM.PipelineManagement.Contracts.DTOs;
using NetMetric.CRM.Sales;
using NetMetric.CRM.Types;
using NetMetric.CurrentUser;
using NetMetric.Exceptions;

namespace NetMetric.CRM.PipelineManagement.Application.Handlers;

public sealed class ConvertLeadCommandHandler(
    IPipelineManagementDbContext dbContext,
    ICurrentUserService currentUserService,
    IOpportunityCrossWriterService opportunityCrossWriterService)
    : IRequestHandler<ConvertLeadCommand, LeadConversionResultDto>
{
    public async Task<LeadConversionResultDto> Handle(ConvertLeadCommand request, CancellationToken cancellationToken)
    {
        currentUserService.EnsureAuthenticated();
        var tenantId = currentUserService.EnsureTenant();

        var lead = await dbContext.Leads.FirstOrDefaultAsync(
                x => x.Id == request.LeadId && x.TenantId == tenantId,
                cancellationToken)
            ?? throw new NotFoundAppException("Lead not found.");

        Customer? customer = null;
        if (request.ExistingCustomerId.HasValue)
        {
            customer = await dbContext.Customers.FirstOrDefaultAsync(
                    x => x.Id == request.ExistingCustomerId.Value && x.TenantId == tenantId,
                    cancellationToken)
                ?? throw new NotFoundAppException("Customer not found.");
        }

        if (request.CreateCustomer)
        {
            var (firstName, lastName) = PipelineDefaults.SplitFullName(lead.FullName);
            customer = new Customer
            {
                TenantId = tenantId,
                FirstName = firstName,
                LastName = string.IsNullOrWhiteSpace(lastName) ? "-" : lastName,
                Email = lead.Email,
                JobTitle = lead.JobTitle,
                Description = string.IsNullOrWhiteSpace(request.Notes) ? lead.Description : request.Notes,
                CompanyId = lead.CompanyId,
                OwnerUserId = request.OwnerUserId ?? lead.OwnerUserId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = currentUserService.UserName,
                UpdatedBy = currentUserService.UserName
            };
            await dbContext.Customers.AddAsync(customer, cancellationToken);
        }

        Guid? opportunityId = null;
        if (request.CreateOpportunity)
        {
            var targetCustomerId = customer?.Id ?? request.ExistingCustomerId;
            if (!targetCustomerId.HasValue)
                throw new ConflictAppException("Opportunity creation requires a customer.");

            opportunityId = await opportunityCrossWriterService.CreateAsync(
                new OpportunityCrossWriterCreateRequest(
                    tenantId,
                    $"OPP-{DateTime.UtcNow:yyyyMMddHHmmss}",
                    string.IsNullOrWhiteSpace(request.OpportunityName) ? $"{lead.FullName} Opportunity" : request.OpportunityName.Trim(),
                    string.IsNullOrWhiteSpace(request.Notes) ? lead.Description : request.Notes,
                    request.EstimatedAmount ?? lead.EstimatedBudget ?? 0m,
                    request.EstimatedAmount ?? lead.EstimatedBudget,
                    PipelineDefaults.ResolveProbability(request.InitialStage),
                    DateTime.UtcNow.AddDays(30),
                    request.InitialStage,
                    PipelineDefaults.ResolveStatus(request.InitialStage),
                    request.Priority,
                    lead.Id,
                    targetCustomerId,
                    request.OwnerUserId ?? lead.OwnerUserId,
                    null,
                    null,
                    null,
                    null,
                    currentUserService.UserName),
                cancellationToken);
        }

        if (customer is not null)
            lead.ConvertedCustomerId = customer.Id;

        lead.Status = LeadStatusType.Won;
        lead.UpdatedAt = DateTime.UtcNow;
        lead.UpdatedBy = currentUserService.UserName;

        await dbContext.SaveChangesAsync(cancellationToken);

        return new LeadConversionResultDto(
            lead.Id,
            customer?.Id ?? request.ExistingCustomerId,
            opportunityId,
            lead.Status,
            "Lead conversion completed successfully.");
    }
}
