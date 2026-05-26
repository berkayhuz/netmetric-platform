// <copyright file="LeadAdministrationService.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using NetMetric.Clock;
using NetMetric.CRM.Core;
using NetMetric.CRM.LeadManagement.Application.Abstractions.Integration;
using NetMetric.CRM.LeadManagement.Application.Abstractions.Services;
using NetMetric.CRM.LeadManagement.Application.Commands.Leads;
using NetMetric.CRM.LeadManagement.Application.Common;
using NetMetric.CRM.LeadManagement.Application.Features.Bulk.Commands.BulkAssignLeadsOwner;
using NetMetric.CRM.LeadManagement.Application.Features.Bulk.Commands.BulkSoftDeleteLeads;
using NetMetric.CRM.LeadManagement.Application.Features.Conversions.Commands.ConvertLeadToCustomer;
using NetMetric.CRM.LeadManagement.Contracts.DTOs;
using NetMetric.CRM.LeadManagement.Domain.Common;
using NetMetric.CRM.OpportunityManagement.Contracts.Integration;
using NetMetric.CRM.LeadManagement.Infrastructure.Persistence;
using NetMetric.CRM.Sales;
using NetMetric.CRM.Types;
using NetMetric.CurrentUser;
using NetMetric.Exceptions;

namespace NetMetric.CRM.LeadManagement.Infrastructure.Services;

public sealed class LeadAdministrationService(
    LeadManagementDbContext dbContext,
    ICurrentUserService currentUserService,
    IClock clock,
    ILeadManagementOutbox outbox,
    IOpportunityCrossWriterService opportunityCrossWriterService) : ILeadAdministrationService
{
    public async Task<LeadDetailDto> CreateAsync(CreateLeadCommand request, CancellationToken cancellationToken)
    {
        currentUserService.EnsureAuthenticated();

        var tenantId = currentUserService.TenantId;
        var actor = ResolveActor();
        var fullName = RequireFullName(request.FullName);

        var lead = new Lead
        {
            TenantId = tenantId,
            LeadCode = LeadManagementMappingExtensions.GenerateLeadCode(),
            FullName = fullName,
            CompanyName = Normalize(request.CompanyName),
            Email = Normalize(request.Email),
            Phone = Normalize(request.Phone),
            JobTitle = Normalize(request.JobTitle),
            Description = Normalize(request.Description),
            EstimatedBudget = request.EstimatedBudget,
            NextContactDate = request.NextContactDate,
            Source = request.Source,
            Status = request.Status,
            Priority = request.Priority,
            CompanyId = request.CompanyId,
            OwnerUserId = request.OwnerUserId,
            CreatedAt = clock.UtcDateTime,
            UpdatedAt = clock.UtcDateTime,
            CreatedBy = actor,
            UpdatedBy = actor
        };

        lead.SetNotes(request.Notes);

        dbContext.Leads.Add(lead);
        await outbox.EnqueueLeadCreatedAsync(lead, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        return lead.ToDetailDto([]);
    }

    public async Task<LeadDetailDto> UpdateAsync(UpdateLeadCommand request, CancellationToken cancellationToken)
    {
        currentUserService.EnsureAuthenticated();

        var lead = await GetLeadForWriteAsync(request.LeadId, cancellationToken);
        ConcurrencyHelper.ApplyRowVersion(dbContext, lead, request.RowVersion);
        var fullName = RequireFullName(request.FullName);

        lead.FullName = fullName;
        lead.CompanyName = Normalize(request.CompanyName);
        lead.Email = Normalize(request.Email);
        lead.Phone = Normalize(request.Phone);
        lead.JobTitle = Normalize(request.JobTitle);
        lead.Description = Normalize(request.Description);
        lead.EstimatedBudget = request.EstimatedBudget;
        lead.NextContactDate = request.NextContactDate;
        lead.Source = request.Source;
        lead.Status = request.Status;
        lead.Priority = request.Priority;
        lead.CompanyId = request.CompanyId;
        lead.OwnerUserId = request.OwnerUserId;
        lead.SetNotes(request.Notes);
        lead.UpdatedAt = clock.UtcDateTime;
        lead.UpdatedBy = ResolveActor();

        if (request.Status is LeadStatusType.Lost or LeadStatusType.Archived)
            lead.Deactivate();
        else
            lead.Activate();

        await outbox.EnqueueLeadUpdatedAsync(lead, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        var scores = await dbContext.LeadScores
            .AsNoTracking()
            .Where(x => x.TenantId == lead.TenantId && x.LeadId == lead.Id)
            .OrderByDescending(x => x.CalculatedAt)
            .ToListAsync(cancellationToken);

        return lead.ToDetailDto(scores);
    }

    public async Task ChangeStatusAsync(ChangeLeadStatusCommand request, CancellationToken cancellationToken)
    {
        currentUserService.EnsureAuthenticated();

        var lead = await GetLeadForWriteAsync(request.LeadId, cancellationToken);
        lead.Status = request.Status;
        lead.UpdatedAt = clock.UtcDateTime;
        lead.UpdatedBy = ResolveActor();

        if (request.Status is LeadStatusType.Lost or LeadStatusType.Archived)
            lead.Deactivate();
        else
            lead.Activate();

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task AssignOwnerAsync(AssignLeadOwnerCommand request, CancellationToken cancellationToken)
    {
        currentUserService.EnsureAuthenticated();

        var lead = await GetLeadForWriteAsync(request.LeadId, cancellationToken);
        lead.OwnerUserId = request.OwnerUserId;
        lead.UpdatedAt = clock.UtcDateTime;
        lead.UpdatedBy = ResolveActor();

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task ScheduleNextContactAsync(ScheduleNextContactCommand request, CancellationToken cancellationToken)
    {
        currentUserService.EnsureAuthenticated();

        var lead = await GetLeadForWriteAsync(request.LeadId, cancellationToken);
        lead.NextContactDate = request.NextContactDate;
        lead.UpdatedAt = clock.UtcDateTime;
        lead.UpdatedBy = ResolveActor();

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<LeadScoreDto> UpsertScoreAsync(UpsertLeadScoreCommand request, CancellationToken cancellationToken)
    {
        currentUserService.EnsureAuthenticated();

        var lead = await GetLeadForWriteAsync(request.LeadId, cancellationToken);

        var score = new LeadScore
        {
            TenantId = lead.TenantId,
            LeadId = lead.Id,
            Score = request.Score,
            ScoreReason = Normalize(request.ScoreReason),
            CalculatedAt = clock.UtcDateTime,
            CreatedAt = clock.UtcDateTime,
            UpdatedAt = clock.UtcDateTime,
            CreatedBy = ResolveActor(),
            UpdatedBy = ResolveActor()
        };

        dbContext.LeadScores.Add(score);

        lead.UpdatedAt = clock.UtcDateTime;
        lead.UpdatedBy = ResolveActor();

        await dbContext.SaveChangesAsync(cancellationToken);

        return score.ToDto();
    }

    public async Task SoftDeleteAsync(SoftDeleteLeadCommand request, CancellationToken cancellationToken)
    {
        currentUserService.EnsureAuthenticated();

        var lead = await GetLeadForWriteAsync(request.LeadId, cancellationToken);
        await AddLeadTrashItemIfMissingAsync(lead, cancellationToken);
        dbContext.Leads.Remove(lead);
        await outbox.EnqueueLeadDeletedAsync(lead, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> BulkAssignOwnerAsync(BulkAssignLeadsOwnerCommand request, CancellationToken cancellationToken)
    {
        currentUserService.EnsureAuthenticated();
        var tenantId = currentUserService.TenantId;

        var leads = await dbContext.Leads
            .Where(x => x.TenantId == tenantId && request.LeadIds.Contains(x.Id))
            .ToListAsync(cancellationToken);

        foreach (var lead in leads)
        {
            lead.OwnerUserId = request.OwnerUserId;
            lead.UpdatedAt = clock.UtcDateTime;
            lead.UpdatedBy = ResolveActor();
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return leads.Count;
    }

    public async Task<int> BulkSoftDeleteAsync(BulkSoftDeleteLeadsCommand request, CancellationToken cancellationToken)
    {
        currentUserService.EnsureAuthenticated();
        var tenantId = currentUserService.TenantId;

        var leads = await dbContext.Leads
            .Where(x => x.TenantId == tenantId && request.LeadIds.Contains(x.Id))
            .ToListAsync(cancellationToken);

        foreach (var lead in leads)
        {
            await AddLeadTrashItemIfMissingAsync(lead, cancellationToken);
        }

        dbContext.Leads.RemoveRange(leads);
        await dbContext.SaveChangesAsync(cancellationToken);

        return leads.Count;
    }

    public async Task<LeadConversionResultDto> ConvertToCustomerAsync(ConvertLeadToCustomerCommand request, CancellationToken cancellationToken)
    {
        currentUserService.EnsureAuthenticated();

        var lead = await GetLeadForWriteAsync(request.LeadId, cancellationToken);

        if (lead.ConvertedCustomerId.HasValue)
            throw new ConflictAppException("Lead is already converted.");

        var (firstName, lastName) = LeadManagementMappingExtensions.SplitName(lead.FullName);

        var customer = new Customer
        {
            TenantId = lead.TenantId,
            FirstName = firstName,
            LastName = lastName,
            Email = lead.Email,
            MobilePhone = lead.Phone,
            JobTitle = lead.JobTitle,
            Description = lead.Description,
            OwnerUserId = lead.OwnerUserId,
            CompanyId = request.CompanyId ?? lead.CompanyId,
            CustomerType = request.CustomerType,
            IsVip = request.MarkCustomerAsVip,
            CreatedAt = clock.UtcDateTime,
            UpdatedAt = clock.UtcDateTime,
            CreatedBy = ResolveActor(),
            UpdatedBy = ResolveActor()
        };

        customer.SetNotes(lead.Notes);

        dbContext.Customers.Add(customer);

        Guid? opportunityId = null;

        if (request.CreateOpportunity)
        {
            opportunityId = await opportunityCrossWriterService.CreateAsync(
                new OpportunityCrossWriterCreateRequest(
                    lead.TenantId,
                    LeadManagementMappingExtensions.GenerateOpportunityCode(),
                    Normalize(request.OpportunityName) ?? $"{lead.FullName} Opportunity",
                    lead.Description,
                    request.EstimatedAmount ?? lead.EstimatedBudget ?? 0m,
                    request.EstimatedAmount ?? lead.EstimatedBudget,
                    LeadConversionDefaults.DefaultOpportunityProbability,
                    lead.NextContactDate?.AddDays(30),
                    OpportunityStageType.Prospecting,
                    OpportunityStatusType.Open,
                    lead.Priority,
                    lead.Id,
                    customer.Id,
                    lead.OwnerUserId,
                    $"Created from lead {lead.LeadCode}.",
                    null,
                    null,
                    null,
                    ResolveActor()),
                cancellationToken);
        }

        lead.ConvertedCustomerId = customer.Id;
        lead.Status = LeadStatusType.Won;
        lead.Deactivate();
        lead.UpdatedAt = clock.UtcDateTime;
        lead.UpdatedBy = ResolveActor();

        await dbContext.SaveChangesAsync(cancellationToken);

        return new LeadConversionResultDto(
            lead.Id,
            customer.Id,
            opportunityId,
            lead.Status.ToString());
    }

    public async Task UpsertQualificationAsync(UpsertLeadQualificationCommand request, CancellationToken cancellationToken)
    {
        currentUserService.EnsureAuthenticated();

        var lead = await GetLeadForWriteAsync(request.LeadId, cancellationToken);

        lead.QualificationFramework = request.FrameworkType;
        lead.QualificationData = request.QualificationDataJson;
        lead.UpdatedAt = clock.UtcDateTime;
        lead.UpdatedBy = ResolveActor();

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<Lead> GetLeadForWriteAsync(Guid leadId, CancellationToken cancellationToken)
    {
        var tenantId = currentUserService.TenantId;

        var lead = await dbContext.Leads
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == leadId, cancellationToken);

        return lead ?? throw new NotFoundAppException("Lead not found.");
    }

    private string ResolveActor()
        => currentUserService.UserName ?? currentUserService.Email ?? "system";

    private static string? Normalize(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string RequireFullName(string? fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName))
        {
            throw new ValidationAppException("FullName is required.");
        }

        return fullName.Trim();
    }

    private async Task AddLeadTrashItemIfMissingAsync(Lead lead, CancellationToken cancellationToken)
    {
        var exists = dbContext.ChangeTracker.Entries<GlobalTrashItem>()
            .Any(x =>
                x.State != EntityState.Deleted
                && x.Entity.TenantId == lead.TenantId
                && x.Entity.EntityType == CrmTrashEntityTypes.Lead
                && x.Entity.EntityId == lead.Id
                && x.Entity.Status == CrmTrashStatuses.Active);

        if (!exists)
        {
            exists = await dbContext.Set<GlobalTrashItem>()
                .AnyAsync(
                    x => x.TenantId == lead.TenantId
                         && x.EntityType == CrmTrashEntityTypes.Lead
                         && x.EntityId == lead.Id
                         && x.Status == CrmTrashStatuses.Active,
                    cancellationToken);
        }

        if (exists)
        {
            return;
        }

        var deletedAtUtc = DateTime.UtcNow;
        var displayName = !string.IsNullOrWhiteSpace(lead.FullName)
            ? lead.FullName.Trim()
            : !string.IsNullOrWhiteSpace(lead.Email)
                ? lead.Email!.Trim()
                : "Deleted lead";
        var summary = !string.IsNullOrWhiteSpace(lead.CompanyName)
            ? lead.CompanyName.Trim()
            : string.IsNullOrWhiteSpace(lead.Email) ? null : lead.Email.Trim();

        await dbContext.Set<GlobalTrashItem>().AddAsync(
            new GlobalTrashItem
            {
                TenantId = lead.TenantId,
                EntityType = CrmTrashEntityTypes.Lead,
                EntityId = lead.Id,
                DisplayName = displayName,
                Summary = summary,
                SourceModule = "leads",
                OriginalRoute = $"/leads/{lead.Id:D}",
                DeletedAtUtc = deletedAtUtc,
                DeletedByUserId = currentUserService.UserId == Guid.Empty ? null : currentUserService.UserId,
                DeletedByDisplayName = currentUserService.UserName ?? currentUserService.Email,
                ExpiresAtUtc = deletedAtUtc.AddDays(7),
                Status = CrmTrashStatuses.Active,
                AuditCorrelationId = Activity.Current?.TraceId.ToString()
            },
            cancellationToken);
    }
}
