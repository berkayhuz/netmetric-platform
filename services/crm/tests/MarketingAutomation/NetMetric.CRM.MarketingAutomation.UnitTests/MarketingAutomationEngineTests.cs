// <copyright file="MarketingAutomationEngineTests.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NetMetric.CRM.MarketingAutomation.Application.Abstractions;
using NetMetric.CRM.MarketingAutomation.Domain.Entities.Attribution;
using NetMetric.CRM.MarketingAutomation.Domain.Entities.CampaignMembers;
using NetMetric.CRM.MarketingAutomation.Domain.Entities.Campaigns;
using NetMetric.CRM.MarketingAutomation.Domain.Entities.Consents;
using NetMetric.CRM.MarketingAutomation.Domain.Entities.Deliveries;
using NetMetric.CRM.MarketingAutomation.Domain.Entities.Journeys;
using NetMetric.CRM.MarketingAutomation.Domain.Entities.LeadNurturing;
using NetMetric.CRM.MarketingAutomation.Domain.Entities.Suppression;
using NetMetric.CRM.MarketingAutomation.Infrastructure.Persistence;
using NetMetric.CRM.MarketingAutomation.Infrastructure.Processing;
using NetMetric.CurrentUser;
using NetMetric.Persistence.EntityFrameworkCore.Auditing;
using NetMetric.Persistence.EntityFrameworkCore.SoftDelete;
using NetMetric.Persistence.EntityFrameworkCore.Tenancy;
using NetMetric.Tenancy;

namespace NetMetric.CRM.MarketingAutomation.UnitTests;

public sealed class MarketingAutomationEngineTests
{
    [Fact]
    public void Dynamic_segment_evaluation_matches_nested_criteria()
    {
        var evaluator = new MarketingSegmentEvaluator();
        var result = evaluator.Evaluate(Guid.NewGuid(), """{"operator":"and","conditions":[{"field":"country","comparison":"eq","value":"TR"},{"field":"score","comparison":"in","value":[80,90]}]}""", [
            new MarketingAudienceMemberInput("ada@example.com", Guid.NewGuid(), """{"country":"TR","score":90}"""),
            new MarketingAudienceMemberInput("ben@example.com", Guid.NewGuid(), """{"country":"US","score":90}""")
        ]);

        result.MatchedCount.Should().Be(1);
        result.Members.Single().EmailAddress.Should().Be("ada@example.com");
    }

    [Fact]
    public async Task Consent_enforcement_blocks_missing_consent()
    {
        await using var fixture = await MarketingFixture.CreateAsync(Guid.NewGuid());
        var service = new MarketingConsentEnforcementService(fixture.Context);

        var result = await service.CheckAsync(fixture.TenantId, "ada@example.com", null, CancellationToken.None);

        result.Allowed.Should().BeFalse();
        result.Reason.Should().Contain("consent");
    }

    [Fact]
    public async Task Suppression_list_blocks_delivery_even_with_consent()
    {
        await using var fixture = await MarketingFixture.CreateAsync(Guid.NewGuid());
        await fixture.GrantConsentAsync("ada@example.com");
        fixture.Context.SuppressionEntries.Add(new SuppressionEntry("ada@example.com", MarketingUtilities.HashEmail("ada@example.com"), "complaint", "event") { TenantId = fixture.TenantId });
        await fixture.Context.SaveChangesAsync();

        var result = await new MarketingConsentEnforcementService(fixture.Context).CheckAsync(fixture.TenantId, "ada@example.com", null, CancellationToken.None);

        result.Allowed.Should().BeFalse();
        result.Suppressed.Should().BeTrue();
    }

    [Fact]
    public async Task Unsubscribe_revokes_consent_and_adds_suppression()
    {
        await using var fixture = await MarketingFixture.CreateAsync(Guid.NewGuid());
        var service = fixture.CreateService();

        var consent = await service.UnsubscribeAsync(fixture.TenantId, new MarketingUnsubscribeRequest("ada@example.com", "email-footer"), CancellationToken.None);

        consent.Status.Should().Be(MarketingConsentStatuses.Revoked);
        fixture.Context.SuppressionEntries.Single().Reason.Should().Be("unsubscribe");
    }

    [Fact]
    public async Task Campaign_scheduling_is_idempotent_for_same_member()
    {
        await using var fixture = await MarketingFixture.CreateAsync(Guid.NewGuid());
        var campaign = await fixture.SeedCampaignWithMemberAsync("ada@example.com");
        await fixture.GrantConsentAsync("ada@example.com");
        campaign.Schedule(DateTime.UtcNow.AddSeconds(-1));
        await fixture.Context.SaveChangesAsync();
        var scheduler = fixture.CreateScheduler(emailDeliveryEnabled: false);

        await scheduler.ScheduleDueCampaignsAsync(CancellationToken.None);
        campaign.Schedule(DateTime.UtcNow.AddSeconds(-1));
        await fixture.Context.SaveChangesAsync();
        await scheduler.ScheduleDueCampaignsAsync(CancellationToken.None);

        fixture.Context.MarketingEmailDeliveries.Count().Should().Be(1);
    }

    [Fact]
    public async Task Journey_start_queues_first_step_idempotently()
    {
        await using var fixture = await MarketingFixture.CreateAsync(Guid.NewGuid());
        var journey = new LeadNurturingJourney("welcome", "Welcome", stepDefinitionJson: """[{"key":"entry"}]""") { TenantId = fixture.TenantId };
        fixture.Context.LeadNurturing.Add(journey);
        await fixture.Context.SaveChangesAsync();
        var service = fixture.CreateService();
        var audience = new[] { new MarketingAudienceMemberInput("ada@example.com", null, "{}") };

        await service.StartJourneyAsync(fixture.TenantId, journey.Id, audience, CancellationToken.None);
        await service.StartJourneyAsync(fixture.TenantId, journey.Id, audience, CancellationToken.None);

        fixture.Context.JourneyStepExecutions.Count().Should().Be(1);
    }

    [Fact]
    public async Task Journey_executor_completes_due_step()
    {
        await using var fixture = await MarketingFixture.CreateAsync(Guid.NewGuid());
        fixture.Context.JourneyStepExecutions.Add(new JourneyStepExecution(Guid.NewGuid(), "entry", MarketingUtilities.HashEmail("ada@example.com"), DateTime.UtcNow.AddSeconds(-1), "journey-key") { TenantId = fixture.TenantId });
        await fixture.Context.SaveChangesAsync();

        var processed = await new MarketingJourneyExecutor(fixture.Context, Options.Create(new MarketingAutomationOptions { EngineEnabled = true })).ProcessDueStepsAsync(CancellationToken.None);

        processed.Should().Be(1);
        fixture.Context.JourneyStepExecutions.Single().Status.Should().Be(JourneyStepStatuses.Completed);
    }

    [Fact]
    public async Task Frequency_cap_blocks_recently_sent_recipient()
    {
        await using var fixture = await MarketingFixture.CreateAsync(Guid.NewGuid());
        var campaign = await fixture.SeedCampaignWithMemberAsync("ada@example.com");
        campaign.Update(campaign.Name, campaign.Description, frequencyCapPerContact: 1, frequencyCapWindowDays: 7);
        await fixture.GrantConsentAsync("ada@example.com");
        var sent = new MarketingEmailDelivery(campaign.Id, null, null, MarketingUtilities.HashEmail("ada@example.com"), "sent-key", DateTime.UtcNow, "corr", 3) { TenantId = fixture.TenantId };
        sent.MarkSent(DateTime.UtcNow);
        fixture.Context.MarketingEmailDeliveries.Add(sent);
        await fixture.Context.SaveChangesAsync();

        var result = await new MarketingConsentEnforcementService(fixture.Context).CheckAsync(fixture.TenantId, "ada@example.com", campaign.Id, CancellationToken.None);

        result.Allowed.Should().BeFalse();
        result.Reason.Should().Contain("frequency");
    }

    [Fact]
    public void Template_renderer_generates_utm_ready_content()
    {
        var preview = new MarketingTemplateRenderer().Render("Hi {{firstName}}", "<a href=\"https://x.test?utm_campaign={{utmCampaign}}\">Go</a>", "Text {{firstName}}", """{"firstName":"Ada","utmCampaign":"spring"}""");

        preview.Subject.Should().Be("Hi Ada");
        preview.HtmlBody.Should().Contain("utm_campaign=spring");
    }

    [Fact]
    public async Task Roi_projection_calculates_return_percent()
    {
        await using var fixture = await MarketingFixture.CreateAsync(Guid.NewGuid());
        var campaign = await fixture.SeedCampaignWithMemberAsync("ada@example.com");
        fixture.Context.CampaignAttributions.Add(new CampaignAttribution(campaign.Id, MarketingUtilities.HashEmail("ada@example.com"), "conversion", 300, 100, "newsletter", "email", campaign.UtmCampaign) { TenantId = fixture.TenantId });
        await fixture.Context.SaveChangesAsync();

        var roi = await fixture.CreateService().GetRoiAsync(fixture.TenantId, campaign.Id, CancellationToken.None);

        roi.RoiPercent.Should().Be(200);
    }

    [Fact]
    public async Task Tenant_filter_isolates_campaigns()
    {
        var tenantId = Guid.NewGuid();
        await using var seed = await MarketingFixture.CreateAsync(null);
        seed.Context.Campaigns.Add(new Campaign("tenant", "Tenant") { TenantId = tenantId });
        seed.Context.Campaigns.Add(new Campaign("other", "Other") { TenantId = Guid.NewGuid() });
        await seed.Context.SaveChangesAsync();
        await using var fixture = MarketingFixture.Create(seed.Connection, tenantId);

        fixture.Context.Campaigns.Count().Should().Be(1);
        fixture.Context.Campaigns.Single().Code.Should().Be("tenant");
    }

    [Fact]
    public void Permission_guard_rejects_missing_permission()
    {
        var guard = new MarketingPermissionGuard(new TestCurrentUser(Guid.NewGuid(), Guid.NewGuid(), []));

        var act = () => guard.Ensure("marketing.campaigns.manage");

        act.Should().Throw<UnauthorizedAccessException>();
    }

    private sealed class MarketingFixture : IAsyncDisposable
    {
        private readonly bool _ownsConnection;
        private readonly TestTenantContext _tenantContext;
        private readonly TestCurrentUser _currentUser;

        private MarketingFixture(SqliteConnection connection, MarketingAutomationDbContext context, Guid tenantId, TestTenantContext tenantContext, TestCurrentUser currentUser, bool ownsConnection)
        {
            Connection = connection;
            Context = context;
            TenantId = tenantId;
            _tenantContext = tenantContext;
            _currentUser = currentUser;
            _ownsConnection = ownsConnection;
        }

        public SqliteConnection Connection { get; }
        public MarketingAutomationDbContext Context { get; }
        public Guid TenantId { get; }

        public static async Task<MarketingFixture> CreateAsync(Guid? tenantId)
        {
            var connection = new SqliteConnection("DataSource=:memory:");
            await connection.OpenAsync();
            var fixture = Create(connection, tenantId ?? Guid.Empty, tenantId.HasValue);
            await fixture.Context.Database.EnsureCreatedAsync();
            return fixture;
        }

        public static MarketingFixture Create(SqliteConnection connection, Guid tenantId, bool ownsConnection = false)
        {
            var tenantContext = new TestTenantContext(tenantId == Guid.Empty ? null : tenantId);
            var currentUser = tenantId == Guid.Empty
                ? new TestCurrentUser(Guid.Empty, Guid.Empty, [])
                : new TestCurrentUser(Guid.NewGuid(), tenantId, ["marketing.campaigns.read", "marketing.campaigns.manage"]);
            var options = new DbContextOptionsBuilder<MarketingAutomationDbContext>().UseSqlite(connection).Options;
            var context = new MarketingAutomationDbContext(
                options,
                tenantContext,
                new TenantIsolationSaveChangesInterceptor(tenantContext, null, currentUser),
                new AuditSaveChangesInterceptor(currentUser),
                new SoftDeleteSaveChangesInterceptor(currentUser));
            return new MarketingFixture(connection, context, tenantId, tenantContext, currentUser, ownsConnection);
        }

        public async Task GrantConsentAsync(string emailAddress)
        {
            Context.MarketingConsents.Add(new MarketingConsent(emailAddress, MarketingUtilities.HashEmail(emailAddress), MarketingConsentStatuses.Granted, "test") { TenantId = TenantId });
            await Context.SaveChangesAsync();
        }

        public async Task<Campaign> SeedCampaignWithMemberAsync(string emailAddress)
        {
            var campaign = new Campaign("spring", "Spring") { TenantId = TenantId };
            campaign.Update(campaign.Name, campaign.Description, budgetAmount: 100, expectedRevenueAmount: 300);
            Context.Campaigns.Add(campaign);
            Context.CampaignMembers.Add(new CampaignMember(campaign.Id, emailAddress, MarketingUtilities.HashEmail(emailAddress)) { TenantId = TenantId });
            await Context.SaveChangesAsync();
            return campaign;
        }

        public MarketingAutomationService CreateService()
            => new(Context, new MarketingSegmentEvaluator(), new MarketingTemplateRenderer(), new MarketingPermissionGuard(_currentUser));

        public MarketingCampaignScheduler CreateScheduler(bool emailDeliveryEnabled)
            => new(
                Context,
                new MarketingConsentEnforcementService(Context),
                new DisabledMarketingEmailDeliveryProvider(),
                Options.Create(new MarketingAutomationOptions { EngineEnabled = true, EmailDeliveryEnabled = emailDeliveryEnabled, MaxAttempts = 3, BaseRetryDelaySeconds = 1 }),
                NullLogger<MarketingCampaignScheduler>.Instance);

        public async ValueTask DisposeAsync()
        {
            await Context.DisposeAsync();
            if (_ownsConnection)
            {
                await Connection.DisposeAsync();
            }
        }
    }

    private sealed class TestTenantContext(Guid? tenantId) : ITenantContext
    {
        public Guid? TenantId { get; } = tenantId;
    }

    private sealed class TestCurrentUser(Guid userId, Guid tenantId, IReadOnlyCollection<string> permissions) : ICurrentUserService
    {
        public Guid UserId { get; } = userId;
        public Guid TenantId { get; } = tenantId;
        public bool IsAuthenticated => UserId != Guid.Empty;
        public string? UserName => "marketing-test";
        public string? Email => "marketing-test@example.com";
        public IReadOnlyCollection<string> Roles => [];
        public IReadOnlyCollection<string> Permissions { get; } = permissions;
        public bool IsInRole(string role) => Roles.Contains(role, StringComparer.OrdinalIgnoreCase);
        public bool HasPermission(string permission) => Permissions.Contains(permission, StringComparer.OrdinalIgnoreCase);
    }
}
