// <copyright file="IMarketingAutomationDbContext.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.MarketingAutomation.Domain.Entities.Attribution;
using NetMetric.CRM.MarketingAutomation.Domain.Entities.CampaignMembers;
using NetMetric.CRM.MarketingAutomation.Domain.Entities.Campaigns;
using NetMetric.CRM.MarketingAutomation.Domain.Entities.Consents;
using NetMetric.CRM.MarketingAutomation.Domain.Entities.Deliveries;
using NetMetric.CRM.MarketingAutomation.Domain.Entities.EmailCampaigns;
using NetMetric.CRM.MarketingAutomation.Domain.Entities.Journeys;
using NetMetric.CRM.MarketingAutomation.Domain.Entities.LeadNurturing;
using NetMetric.CRM.MarketingAutomation.Domain.Entities.Segments;
using NetMetric.CRM.MarketingAutomation.Domain.Entities.Suppression;
using NetMetric.Repository;

namespace NetMetric.CRM.MarketingAutomation.Application.Abstractions.Persistence;

public interface IMarketingAutomationDbContext : IUnitOfWork
{
    DbSet<Campaign> Campaigns { get; }
    DbSet<Segment> Segments { get; }
    DbSet<CampaignMember> CampaignMembers { get; }
    DbSet<EmailCampaign> EmailCampaigns { get; }
    DbSet<LeadNurturingJourney> LeadNurturing { get; }
    DbSet<MarketingConsent> MarketingConsents { get; }
    DbSet<SuppressionEntry> SuppressionEntries { get; }
    DbSet<MarketingEmailDelivery> MarketingEmailDeliveries { get; }
    DbSet<JourneyStepExecution> JourneyStepExecutions { get; }
    DbSet<CampaignAttribution> CampaignAttributions { get; }
    DbSet<CampaignRoiProjection> CampaignRoiProjections { get; }
}
