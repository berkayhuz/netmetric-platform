// <copyright file="QuoteOpportunityReadModelSyncService.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.OpportunityManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.QuoteManagement.Application.Abstractions.Services;
using NetMetric.CRM.QuoteManagement.Infrastructure.Persistence;
using NetMetric.CRM.Types;
using NetMetric.CurrentUser;
using NetMetric.Exceptions;

namespace NetMetric.CRM.QuoteManagement.Infrastructure.Services;

public sealed class QuoteOpportunityReadModelSyncService(
    QuoteManagementDbContext quoteDbContext,
    IOpportunityManagementDbContext opportunityManagementDbContext,
    ICurrentUserService currentUserService) : IQuoteOpportunityReadModelSyncService
{
    public async Task<QuoteOpportunityReadModelSyncResult> SyncAsync(Guid opportunityId, CancellationToken cancellationToken)
    {
        currentUserService.EnsureAuthenticated();
        var tenantId = currentUserService.EnsureTenant();

        var sourceOpportunity = await opportunityManagementDbContext.Opportunities
            .AsNoTracking()
            .Where(x => x.TenantId == tenantId && x.Id == opportunityId && !x.IsDeleted)
            .Select(x => new OpportunityProjection(
                x.Id,
                x.CustomerId,
                x.OpportunityCode,
                x.Name,
                x.EstimatedAmount,
                x.ExpectedRevenue,
                x.Probability,
                x.Stage,
                x.Status,
                x.ForecastCategory,
                x.Priority,
                x.OwnerUserId,
                x.EstimatedCloseDate,
                x.Description,
                x.Notes))
            .SingleOrDefaultAsync(cancellationToken);

        if (sourceOpportunity is null)
        {
            throw new ValidationAppException(
                "Selected opportunity is not available.",
                new Dictionary<string, string[]>
                {
                    ["opportunityId"] = ["Selected opportunity is not available."]
                });
        }

        var localOpportunity = await quoteDbContext.Opportunities
            .FirstOrDefaultAsync(x => x.Id == opportunityId, cancellationToken);

        var utcNow = DateTime.UtcNow;
        var actor = currentUserService.UserName;

        if (localOpportunity is null)
        {
            localOpportunity = new CRM.Sales.Opportunity
            {
                TenantId = tenantId,
                OpportunityCode = sourceOpportunity.OpportunityCode,
                Name = sourceOpportunity.Name,
                EstimatedAmount = sourceOpportunity.EstimatedAmount,
                ExpectedRevenue = sourceOpportunity.ExpectedRevenue,
                Probability = sourceOpportunity.Probability,
                Stage = sourceOpportunity.Stage,
                Status = sourceOpportunity.Status,
                ForecastCategory = sourceOpportunity.ForecastCategory,
                Priority = sourceOpportunity.Priority,
                OwnerUserId = sourceOpportunity.OwnerUserId,
                EstimatedCloseDate = sourceOpportunity.EstimatedCloseDate,
                Description = sourceOpportunity.Description,
                Notes = sourceOpportunity.Notes,
                CreatedAt = utcNow,
                UpdatedAt = utcNow,
                CreatedBy = actor,
                UpdatedBy = actor,
                LeadId = null,
                CustomerId = null,
                CompanyId = null,
                LostReasonId = null,
                PipelineId = null,
                PipelineStageId = null,
                ClosedAt = null,
                LostNote = null
            };
            localOpportunity.SetNotes(sourceOpportunity.Notes);

            var entry = quoteDbContext.Opportunities.Add(localOpportunity);
            entry.Property(x => x.Id).CurrentValue = sourceOpportunity.Id;
        }
        else
        {
            localOpportunity.TenantId = tenantId;
            localOpportunity.OpportunityCode = sourceOpportunity.OpportunityCode;
            localOpportunity.Name = sourceOpportunity.Name;
            localOpportunity.EstimatedAmount = sourceOpportunity.EstimatedAmount;
            localOpportunity.ExpectedRevenue = sourceOpportunity.ExpectedRevenue;
            localOpportunity.Probability = sourceOpportunity.Probability;
            localOpportunity.Stage = sourceOpportunity.Stage;
            localOpportunity.Status = sourceOpportunity.Status;
            localOpportunity.ForecastCategory = sourceOpportunity.ForecastCategory;
            localOpportunity.Priority = sourceOpportunity.Priority;
            localOpportunity.OwnerUserId = sourceOpportunity.OwnerUserId;
            localOpportunity.EstimatedCloseDate = sourceOpportunity.EstimatedCloseDate;
            localOpportunity.Description = sourceOpportunity.Description;
            localOpportunity.SetNotes(sourceOpportunity.Notes);
            localOpportunity.UpdatedAt = utcNow;
            localOpportunity.UpdatedBy = actor;
        }

        return new QuoteOpportunityReadModelSyncResult(sourceOpportunity.Id, sourceOpportunity.CustomerId);
    }

    private sealed record OpportunityProjection(
        Guid Id,
        Guid? CustomerId,
        string OpportunityCode,
        string Name,
        decimal EstimatedAmount,
        decimal? ExpectedRevenue,
        decimal Probability,
        OpportunityStageType Stage,
        OpportunityStatusType Status,
        ForecastCategory ForecastCategory,
        PriorityType Priority,
        Guid? OwnerUserId,
        DateTime? EstimatedCloseDate,
        string? Description,
        string? Notes);
}
