// <copyright file="LeadRoutingService.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NetMetric.CRM.LeadManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.LeadManagement.Application.Abstractions.Services;
using NetMetric.CRM.Sales;
using NetMetric.CurrentUser;

namespace NetMetric.CRM.LeadManagement.Infrastructure.Services;

public sealed class LeadRoutingService(
    ILeadManagementDbContext dbContext,
    ICurrentUserService currentUserService,
    ILogger<LeadRoutingService> logger) : ILeadRoutingService
{
    public async Task RouteLeadAsync(Guid leadId, CancellationToken cancellationToken)
    {
        var lead = await dbContext.Leads
            .Include(x => x.OwnershipHistories)
            .FirstOrDefaultAsync(x => x.Id == leadId, cancellationToken);

        if (lead is null)
        {
            logger.LogWarning("Lead not found for routing. LeadId: {LeadId}", leadId);
            return;
        }

        if (lead.OwnerUserId.HasValue)
        {
            logger.LogInformation("Lead {LeadId} is already assigned to {OwnerId}. Skipping routing.", leadId, lead.OwnerUserId);
            return;
        }

        // --- Demo Routing Logic ---
        // In a real scenario, this would load RoutingRules from the database (e.g., matching by Geography, Source, Industry)
        // and evaluate them. Here we simulate assigning it to a default user or based on Source.

        Guid? assignedUserId = ResolveOwnerByRules();
        string ruleId = "DefaultRule_V1";
        string assignmentReason = "Assigned via auto-routing based on fallback logic.";

        if (!assignedUserId.HasValue)
        {
            logger.LogWarning("No suitable routing rule matched for LeadId: {LeadId}", leadId);
            return; // Or assign to a default catch-all queue
        }

        // Apply Assignment
        var previousOwner = lead.OwnerUserId;
        lead.OwnerUserId = assignedUserId;

        // Log Ownership History
        var history = new LeadOwnershipHistory
        {
            LeadId = lead.Id,
            PreviousOwnerId = previousOwner,
            NewOwnerId = assignedUserId,
            AssignmentRuleId = ruleId,
            AssignmentReason = assignmentReason,
            AssignedAt = DateTime.UtcNow,
            AssignedByUserId = currentUserService.UserId, // System user ID usually
            TenantId = lead.TenantId
        };

        lead.OwnershipHistories.Add(history);

        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Successfully routed Lead {LeadId} to User {UserId}", leadId, assignedUserId);
    }

    private static Guid? ResolveOwnerByRules()
    {
        // Dummy fallback GUID for demo
        return Guid.NewGuid();
    }
}
