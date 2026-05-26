// <copyright file="LeadCaptureService.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System.Text.Json;
using MediatR;
using Microsoft.Extensions.Logging;

using NetMetric.CRM.LeadManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.LeadManagement.Application.Abstractions.Services;
using NetMetric.CRM.LeadManagement.Application.Commands.Leads;
using NetMetric.CRM.Sales;
using NetMetric.CRM.Types;
using NetMetric.Tenancy;

namespace NetMetric.CRM.LeadManagement.Infrastructure.Services;

public sealed class LeadCaptureService(
    ILeadManagementDbContext dbContext,
    ITenantProvider tenantProvider,
    IPublisher publisher,
    ILogger<LeadCaptureService> logger) : ILeadCaptureService
{
    public async Task<Guid> CaptureAsync(CaptureLeadCommand request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Capturing new lead from source: {Source}", request.Source);

        var tenantId = tenantProvider.TenantId ?? throw new UnauthorizedAccessException("Tenant is required.");

        // 1. Basic mapping
        var lead = new Lead
        {
            TenantId = tenantId,
            FullName = request.FullName,
            Email = request.Email,
            Phone = request.Phone,
            CompanyName = request.CompanyName,
            JobTitle = request.JobTitle,
            Description = request.Description,
            Source = request.Source,
            Status = LeadStatusType.New,
            Priority = PriorityType.Medium,
            LeadCode = $"L-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..6].ToUpper()}",
            CaptureFormId = request.CaptureFormId,
            ReferrerUrl = request.ReferrerUrl,
            UtmSource = request.UtmSource,
            UtmMedium = request.UtmMedium,
            UtmCampaign = request.UtmCampaign,
            UtmTerm = request.UtmTerm,
            UtmContent = request.UtmContent,
            SlaTargetTime = DateTime.UtcNow.AddMinutes(15), // SLA target: contact within 15 mins by default
            IsSpam = EvaluateSpam(request)
        };

        // 2. Parse DynamicData if needed (e.g., custom mapped fields)
        if (request.DynamicData is not null && request.DynamicData.Count != 0)
        {
            var dynamicJson = JsonSerializer.Serialize(request.DynamicData);
            lead.SetNotes($"Captured dynamic data: {dynamicJson}");
        }

        // 3. Save to database
        dbContext.Leads.Add(lead);
        await dbContext.SaveChangesAsync(cancellationToken);

        // 4. Publish event to trigger Routing & AI Scoring consumers
        await publisher.Publish(new LeadCapturedNotification(lead.Id), cancellationToken);

        return lead.Id;
    }

    private static bool EvaluateSpam(CaptureLeadCommand request)
    {
        // Simple heuristic for demo. In a real scenario, integrate with reCAPTCHA or Akismet
        if (string.IsNullOrWhiteSpace(request.Email) && string.IsNullOrWhiteSpace(request.Phone))
        {
            return true;
        }

        if (request.FullName.Contains("test", StringComparison.OrdinalIgnoreCase) ||
            request.FullName.Contains("spam", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return false;
    }
}
