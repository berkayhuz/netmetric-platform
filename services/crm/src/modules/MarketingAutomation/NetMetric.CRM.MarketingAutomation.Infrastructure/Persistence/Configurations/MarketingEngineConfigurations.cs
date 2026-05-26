// <copyright file="MarketingEngineConfigurations.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NetMetric.CRM.MarketingAutomation.Domain.Entities.Attribution;
using NetMetric.CRM.MarketingAutomation.Domain.Entities.Consents;
using NetMetric.CRM.MarketingAutomation.Domain.Entities.Deliveries;
using NetMetric.CRM.MarketingAutomation.Domain.Entities.Journeys;
using NetMetric.CRM.MarketingAutomation.Domain.Entities.Suppression;

namespace NetMetric.CRM.MarketingAutomation.Infrastructure.Persistence.Configurations;

public sealed class MarketingConsentConfiguration : IEntityTypeConfiguration<MarketingConsent>
{
    public void Configure(EntityTypeBuilder<MarketingConsent> builder)
    {
        builder.ToTable("MarketingConsent");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.EmailAddress).HasMaxLength(320).IsRequired();
        builder.Property(x => x.EmailHash).HasMaxLength(128).IsRequired();
        builder.Property(x => x.Status).HasMaxLength(80).IsRequired();
        builder.Property(x => x.Source).HasMaxLength(200).IsRequired();
        builder.Property(x => x.DoubleOptInTokenHash).HasMaxLength(256);
        builder.HasIndex(x => new { x.TenantId, x.EmailHash }).IsUnique().HasFilter("[IsDeleted] = 0");
    }
}

public sealed class SuppressionEntryConfiguration : IEntityTypeConfiguration<SuppressionEntry>
{
    public void Configure(EntityTypeBuilder<SuppressionEntry> builder)
    {
        builder.ToTable("SuppressionEntry");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.EmailAddress).HasMaxLength(320).IsRequired();
        builder.Property(x => x.EmailHash).HasMaxLength(128).IsRequired();
        builder.Property(x => x.Reason).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Source).HasMaxLength(200).IsRequired();
        builder.HasIndex(x => new { x.TenantId, x.EmailHash }).IsUnique().HasFilter("[IsDeleted] = 0");
    }
}

public sealed class MarketingEmailDeliveryConfiguration : IEntityTypeConfiguration<MarketingEmailDelivery>
{
    public void Configure(EntityTypeBuilder<MarketingEmailDelivery> builder)
    {
        builder.ToTable("MarketingEmailDelivery");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.EmailHash).HasMaxLength(128).IsRequired();
        builder.Property(x => x.Status).HasMaxLength(50).IsRequired();
        builder.Property(x => x.IdempotencyKey).HasMaxLength(500).IsRequired();
        builder.Property(x => x.CorrelationId).HasMaxLength(200).IsRequired();
        builder.Property(x => x.FailureCode).HasMaxLength(100);
        builder.Property(x => x.FailureMessage).HasMaxLength(1000);
        builder.HasIndex(x => new { x.TenantId, x.IdempotencyKey }).IsUnique();
        builder.HasIndex(x => new { x.TenantId, x.Status, x.NextAttemptAtUtc });
        builder.HasIndex(x => new { x.TenantId, x.EmailHash, x.SentAtUtc });
    }
}

public sealed class JourneyStepExecutionConfiguration : IEntityTypeConfiguration<JourneyStepExecution>
{
    public void Configure(EntityTypeBuilder<JourneyStepExecution> builder)
    {
        builder.ToTable("JourneyStepExecution");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.StepKey).HasMaxLength(100).IsRequired();
        builder.Property(x => x.EmailHash).HasMaxLength(128).IsRequired();
        builder.Property(x => x.Status).HasMaxLength(50).IsRequired();
        builder.Property(x => x.IdempotencyKey).HasMaxLength(500).IsRequired();
        builder.Property(x => x.ErrorMessage).HasMaxLength(1000);
        builder.HasIndex(x => new { x.TenantId, x.IdempotencyKey }).IsUnique();
        builder.HasIndex(x => new { x.TenantId, x.Status, x.NextAttemptAtUtc });
    }
}

public sealed class CampaignAttributionConfiguration : IEntityTypeConfiguration<CampaignAttribution>
{
    public void Configure(EntityTypeBuilder<CampaignAttribution> builder)
    {
        builder.ToTable("CampaignAttribution");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.EmailHash).HasMaxLength(128).IsRequired();
        builder.Property(x => x.TouchType).HasMaxLength(80).IsRequired();
        builder.Property(x => x.UtmSource).HasMaxLength(120).IsRequired();
        builder.Property(x => x.UtmMedium).HasMaxLength(120).IsRequired();
        builder.Property(x => x.UtmCampaign).HasMaxLength(200).IsRequired();
        builder.Property(x => x.RevenueAmount).HasPrecision(18, 2);
        builder.Property(x => x.CostAmount).HasPrecision(18, 2);
        builder.HasIndex(x => new { x.TenantId, x.CampaignId, x.OccurredAtUtc });
    }
}

public sealed class CampaignRoiProjectionConfiguration : IEntityTypeConfiguration<CampaignRoiProjection>
{
    public void Configure(EntityTypeBuilder<CampaignRoiProjection> builder)
    {
        builder.ToTable("CampaignRoiProjection");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.RevenueAmount).HasPrecision(18, 2);
        builder.Property(x => x.CostAmount).HasPrecision(18, 2);
        builder.Property(x => x.RoiPercent).HasPrecision(18, 2);
        builder.HasIndex(x => new { x.TenantId, x.CampaignId }).IsUnique();
    }
}
