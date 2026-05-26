// <copyright file="ILeadAdministrationService.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.CRM.LeadManagement.Application.Commands.Leads;
using NetMetric.CRM.LeadManagement.Application.Features.Bulk.Commands.BulkAssignLeadsOwner;
using NetMetric.CRM.LeadManagement.Application.Features.Bulk.Commands.BulkSoftDeleteLeads;
using NetMetric.CRM.LeadManagement.Application.Features.Conversions.Commands.ConvertLeadToCustomer;
using NetMetric.CRM.LeadManagement.Contracts.DTOs;

namespace NetMetric.CRM.LeadManagement.Application.Abstractions.Services;

public interface ILeadAdministrationService
{
    Task<LeadDetailDto> CreateAsync(CreateLeadCommand request, CancellationToken cancellationToken);
    Task<LeadDetailDto> UpdateAsync(UpdateLeadCommand request, CancellationToken cancellationToken);
    Task ChangeStatusAsync(ChangeLeadStatusCommand request, CancellationToken cancellationToken);
    Task AssignOwnerAsync(AssignLeadOwnerCommand request, CancellationToken cancellationToken);
    Task ScheduleNextContactAsync(ScheduleNextContactCommand request, CancellationToken cancellationToken);
    Task<LeadScoreDto> UpsertScoreAsync(UpsertLeadScoreCommand request, CancellationToken cancellationToken);
    Task SoftDeleteAsync(SoftDeleteLeadCommand request, CancellationToken cancellationToken);
    Task<int> BulkAssignOwnerAsync(BulkAssignLeadsOwnerCommand request, CancellationToken cancellationToken);
    Task<int> BulkSoftDeleteAsync(BulkSoftDeleteLeadsCommand request, CancellationToken cancellationToken);
    Task<LeadConversionResultDto> ConvertToCustomerAsync(ConvertLeadToCustomerCommand request, CancellationToken cancellationToken);
    Task UpsertQualificationAsync(UpsertLeadQualificationCommand request, CancellationToken cancellationToken);
}
