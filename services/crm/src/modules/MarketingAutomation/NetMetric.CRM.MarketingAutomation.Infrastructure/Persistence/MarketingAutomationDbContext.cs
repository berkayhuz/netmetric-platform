// <copyright file="MarketingAutomationDbContext.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.MarketingAutomation.Application.Abstractions.Persistence;
using NetMetric.CRM.MarketingAutomation.Domain.Entities.Attribution;
using NetMetric.CRM.MarketingAutomation.Domain.Entities.CampaignMembers;
using NetMetric.CRM.MarketingAutomation.Domain.Entities.Campaigns;
using NetMetric.CRM.MarketingAutomation.Domain.Entities.Captures;
using NetMetric.CRM.MarketingAutomation.Domain.Entities.Consents;
using NetMetric.CRM.MarketingAutomation.Domain.Entities.Deliveries;
using NetMetric.CRM.MarketingAutomation.Domain.Entities.EmailCampaigns;
using NetMetric.CRM.MarketingAutomation.Domain.Entities.Forms;
using NetMetric.CRM.MarketingAutomation.Domain.Entities.Journeys;
using NetMetric.CRM.MarketingAutomation.Domain.Entities.LeadNurturing;
using NetMetric.CRM.MarketingAutomation.Domain.Entities.Segments;
using NetMetric.CRM.MarketingAutomation.Domain.Entities.Suppression;
using NetMetric.CRM.MarketingAutomation.Domain.Entities.UtmSources;
using NetMetric.Persistence.EntityFrameworkCore;
using NetMetric.Persistence.EntityFrameworkCore.Auditing;
using NetMetric.Persistence.EntityFrameworkCore.SoftDelete;
using NetMetric.Persistence.EntityFrameworkCore.Tenancy;
using NetMetric.Tenancy;

namespace NetMetric.CRM.MarketingAutomation.Infrastructure.Persistence;

public sealed class MarketingAutomationDbContext : AppDbContext, IMarketingAutomationDbContext
{
    private readonly TenantIsolationSaveChangesInterceptor _tenantInterceptor;
    private readonly AuditSaveChangesInterceptor _auditInterceptor;
    private readonly SoftDeleteSaveChangesInterceptor _softDeleteInterceptor;

    public MarketingAutomationDbContext(
        DbContextOptions<MarketingAutomationDbContext> options,
        ITenantContext tenantContext,
        TenantIsolationSaveChangesInterceptor tenantInterceptor,
        AuditSaveChangesInterceptor auditInterceptor,
        SoftDeleteSaveChangesInterceptor softDeleteInterceptor) : base(options, tenantContext)
    {
        _tenantInterceptor = tenantInterceptor;
        _auditInterceptor = auditInterceptor;
        _softDeleteInterceptor = softDeleteInterceptor;
    }

    public DbSet<Campaign> Campaigns => Set<Campaign>();
    public DbSet<Segment> Segments => Set<Segment>();
    public DbSet<CampaignMember> CampaignMembers => Set<CampaignMember>();
    public DbSet<EmailCampaign> EmailCampaigns => Set<EmailCampaign>();
    public DbSet<LeadNurturingJourney> LeadNurturing => Set<LeadNurturingJourney>();
    public DbSet<UtmSource> UtmSources => Set<UtmSource>();
    public DbSet<LandingForm> Forms => Set<LandingForm>();
    public DbSet<LandingCapture> Captures => Set<LandingCapture>();
    public DbSet<MarketingConsent> MarketingConsents => Set<MarketingConsent>();
    public DbSet<SuppressionEntry> SuppressionEntries => Set<SuppressionEntry>();
    public DbSet<MarketingEmailDelivery> MarketingEmailDeliveries => Set<MarketingEmailDelivery>();
    public DbSet<JourneyStepExecution> JourneyStepExecutions => Set<JourneyStepExecution>();
    public DbSet<CampaignAttribution> CampaignAttributions => Set<CampaignAttribution>();
    public DbSet<CampaignRoiProjection> CampaignRoiProjections => Set<CampaignRoiProjection>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.AddInterceptors(_tenantInterceptor, _auditInterceptor, _softDeleteInterceptor);
        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MarketingAutomationDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
