// <copyright file="IntegrationHubDbContext.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NetMetric.CRM.IntegrationHub.Application.Abstractions.Persistence;
using NetMetric.CRM.IntegrationHub.Domain.Entities;
using NetMetric.Persistence.EntityFrameworkCore;
using NetMetric.Tenancy;

namespace NetMetric.CRM.IntegrationHub.Infrastructure.Persistence;

public sealed class IntegrationHubDbContext(
    DbContextOptions<IntegrationHubDbContext> options,
    ITenantContext tenantContext,
    IDataProtectionProvider dataProtectionProvider)
    : AppDbContext(options, tenantContext), IIntegrationHubDbContext
{
    private readonly IDataProtector _secretProtector =
        dataProtectionProvider.CreateProtector("NetMetric.CRM.IntegrationHub.Secrets.v1");

    public DbSet<IntegrationConnection> IntegrationConnections => Set<IntegrationConnection>();
    public DbSet<IntegrationJob> IntegrationJobs => Set<IntegrationJob>();
    public DbSet<IntegrationLogEntry> IntegrationLogEntries => Set<IntegrationLogEntry>();
    public DbSet<WebhookSubscription> WebhookSubscriptions => Set<WebhookSubscription>();
    public DbSet<IntegrationDeadLetterEntry> IntegrationDeadLetters => Set<IntegrationDeadLetterEntry>();
    public DbSet<IntegrationJobExecutionLog> IntegrationJobExecutionLogs => Set<IntegrationJobExecutionLog>();
    public DbSet<IntegrationWebhookDelivery> WebhookDeliveries => Set<IntegrationWebhookDelivery>();
    public DbSet<ProviderCredential> ProviderCredentials => Set<ProviderCredential>();
    public DbSet<IntegrationApiKey> IntegrationApiKeys => Set<IntegrationApiKey>();
    public DbSet<WebhookDeliveryAttempt> WebhookDeliveryAttempts => Set<WebhookDeliveryAttempt>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<IntegrationConnection>(builder =>
        {
            builder.ToTable("IntegrationConnections");
            builder.HasKey(x => x.Id);
            builder.HasIndex(x => new { x.TenantId, x.ProviderKey }).IsUnique();
            builder.Property(x => x.ProviderKey).HasMaxLength(100).IsRequired();
            builder.Property(x => x.DisplayName).HasMaxLength(200).IsRequired();
            builder.Property(x => x.Category).HasMaxLength(100).IsRequired();
            builder.Property(x => x.SettingsJson).HasConversion(CreateProtectedStringConverter()).IsRequired();
            builder.Property(x => x.HealthStatus).HasMaxLength(40).IsRequired();
            builder.Property(x => x.HealthMessage).HasMaxLength(1000);
            builder.Property(x => x.DeltaSyncToken).HasMaxLength(2000).HasConversion(CreateProtectedNullableStringConverter());
        });

        modelBuilder.Entity<IntegrationJob>(builder =>
        {
            builder.ToTable("IntegrationJobs");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.ProviderKey).HasMaxLength(100).IsRequired();
            builder.Property(x => x.JobType).HasMaxLength(100).IsRequired();
            builder.Property(x => x.Direction).HasMaxLength(20).IsRequired();
            builder.Property(x => x.IdempotencyKey).HasMaxLength(200).IsRequired();
            builder.Property(x => x.Status).HasMaxLength(40).IsRequired();
            builder.Property(x => x.LockedBy).HasMaxLength(200);
            builder.Property(x => x.LastErrorCode).HasMaxLength(100);
            builder.Property(x => x.LastErrorMessage).HasMaxLength(1000);
            builder.Property(x => x.ErrorClassification).HasMaxLength(80);
            builder.Property(x => x.CorrelationId).HasMaxLength(120);
            builder.Property(x => x.CancellationReason).HasMaxLength(500);
            builder.Property(x => x.PayloadJson).HasConversion(CreateProtectedStringConverter()).IsRequired();
            builder.HasIndex(x => new { x.TenantId, x.Status, x.NextAttemptAtUtc });
            builder.HasIndex(x => new { x.TenantId, x.ProviderKey, x.IdempotencyKey }).IsUnique().HasFilter("[IsDeleted] = 0");
        });

        modelBuilder.Entity<IntegrationLogEntry>(builder =>
        {
            builder.ToTable("IntegrationLogEntries");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.ProviderKey).HasMaxLength(100).IsRequired();
            builder.Property(x => x.Direction).HasMaxLength(20).IsRequired();
            builder.Property(x => x.Status).HasMaxLength(20).IsRequired();
            builder.Property(x => x.Message).HasMaxLength(2000).IsRequired();
        });

        modelBuilder.Entity<WebhookSubscription>(builder =>
        {
            builder.ToTable("WebhookSubscriptions");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Name).HasMaxLength(180).IsRequired();
            builder.Property(x => x.EventKey).HasMaxLength(120).IsRequired();
            builder.Property(x => x.TargetUrl).HasMaxLength(2048).HasConversion(CreateProtectedStringConverter()).IsRequired();
            builder.Property(x => x.SecretKey).HasMaxLength(1024).HasConversion(CreateProtectedStringConverter()).IsRequired();
            builder.Property(x => x.TimeoutSeconds).IsRequired();
            builder.Property(x => x.MaxAttempts).IsRequired();
            builder.Property(x => x.LastSuccessAtUtc);
            builder.Property(x => x.LastFailureAtUtc);
            builder.HasIndex(x => new { x.TenantId, x.EventKey }).HasFilter("[IsDeleted] = 0");
        });

        modelBuilder.Entity<IntegrationDeadLetterEntry>(builder =>
        {
            builder.ToTable("IntegrationDeadLetters");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.ProviderKey).HasMaxLength(100).IsRequired();
            builder.Property(x => x.JobType).HasMaxLength(100).IsRequired();
            builder.Property(x => x.Direction).HasMaxLength(20).IsRequired();
            builder.Property(x => x.IdempotencyKey).HasMaxLength(200).IsRequired();
            builder.Property(x => x.PayloadJson).HasConversion(CreateProtectedStringConverter()).IsRequired();
            builder.Property(x => x.ErrorClassification).HasMaxLength(80).IsRequired();
            builder.Property(x => x.ErrorCode).HasMaxLength(100).IsRequired();
            builder.Property(x => x.SanitizedErrorMessage).HasMaxLength(1000).IsRequired();
            builder.Property(x => x.Status).HasMaxLength(40).IsRequired();
            builder.HasIndex(x => new { x.TenantId, x.JobId }).IsUnique().HasFilter("[IsDeleted] = 0");
            builder.HasIndex(x => new { x.TenantId, x.Status, x.FailedAtUtc });
        });

        modelBuilder.Entity<IntegrationJobExecutionLog>(builder =>
        {
            builder.ToTable("IntegrationJobExecutionLogs");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Status).HasMaxLength(40).IsRequired();
            builder.Property(x => x.ErrorClassification).HasMaxLength(80);
            builder.Property(x => x.ErrorCode).HasMaxLength(100);
            builder.Property(x => x.Message).HasMaxLength(1000).IsRequired();
            builder.Property(x => x.CorrelationId).HasMaxLength(120).IsRequired();
            builder.HasIndex(x => new { x.TenantId, x.JobId, x.StartedAtUtc });
        });

        modelBuilder.Entity<IntegrationWebhookDelivery>(builder =>
        {
            builder.ToTable("IntegrationWebhookDeliveries");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.ProviderKey).HasMaxLength(100).IsRequired();
            builder.Property(x => x.EventId).HasMaxLength(200).IsRequired();
            builder.Property(x => x.SignatureHash).HasMaxLength(128).IsRequired();
            builder.Property(x => x.PayloadHash).HasMaxLength(128).IsRequired();
            builder.Property(x => x.Status).HasMaxLength(40).IsRequired();
            builder.Property(x => x.FailureReason).HasMaxLength(500);
            builder.HasIndex(x => new { x.TenantId, x.ProviderKey, x.EventId }).IsUnique().HasFilter("[IsDeleted] = 0");
            builder.HasIndex(x => new { x.TenantId, x.ProviderKey, x.ReceivedAtUtc });
        });

        modelBuilder.Entity<ProviderCredential>(builder =>
        {
            builder.ToTable("ProviderCredentials");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.ProviderKey).HasMaxLength(80).IsRequired();
            builder.Property(x => x.DisplayName).HasMaxLength(180).IsRequired();
            builder.Property(x => x.EndpointKey).HasMaxLength(80).IsRequired();
            builder.Property(x => x.AccessToken).HasMaxLength(2048).HasConversion(CreateProtectedStringConverter()).IsRequired();
            builder.Property(x => x.WebhookSigningSecret).HasMaxLength(1024).HasConversion(CreateProtectedStringConverter()).IsRequired();
            builder.Property(x => x.ConfigurationJson).HasMaxLength(4000).HasConversion(CreateProtectedNullableStringConverter());
            builder.Property(x => x.Status).HasMaxLength(40).IsRequired();
            builder.Property(x => x.LastValidationStatus).HasMaxLength(40);
            builder.Property(x => x.LastValidationCode).HasMaxLength(80);
            builder.Property(x => x.LastValidationMessage).HasMaxLength(500);
            builder.Property(x => x.LastValidatedAtUtc);
            builder.Property(x => x.IsEnabled).IsRequired();
            builder.HasIndex(x => new { x.TenantId, x.ProviderKey, x.DisplayName }).HasFilter("[IsDeleted] = 0");
            builder.HasIndex(x => x.EndpointKey).IsUnique().HasFilter("[IsDeleted] = 0");
        });

        modelBuilder.Entity<IntegrationApiKey>(builder =>
        {
            builder.ToTable("IntegrationApiKeys");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Name).HasMaxLength(120).IsRequired();
            builder.Property(x => x.Description).HasMaxLength(500);
            builder.Property(x => x.KeyPrefix).HasMaxLength(32).IsRequired();
            builder.Property(x => x.KeySalt).HasMaxLength(256).IsRequired();
            builder.Property(x => x.KeyHash).HasMaxLength(256).IsRequired();
            builder.Property(x => x.ExpiresAtUtc);
            builder.Property(x => x.LastUsedAtUtc);
            builder.Property(x => x.RevokedAtUtc);
            builder.Ignore(x => x.IsRevoked);
            builder.HasIndex(x => new { x.TenantId, x.KeyPrefix }).IsUnique().HasFilter("[IsDeleted] = 0");
            builder.HasMany(x => x.Scopes)
                .WithOne()
                .HasForeignKey("IntegrationApiKeyId")
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<IntegrationApiKeyScope>(builder =>
        {
            builder.ToTable("IntegrationApiKeyScopes");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Scope).HasMaxLength(120).IsRequired();
            builder.HasIndex("IntegrationApiKeyId", nameof(IntegrationApiKeyScope.Scope)).IsUnique();
        });

        modelBuilder.Entity<WebhookDeliveryAttempt>(builder =>
        {
            builder.ToTable("WebhookDeliveryAttempts");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.EventId).HasMaxLength(200).IsRequired();
            builder.Property(x => x.EventType).HasMaxLength(120).IsRequired();
            builder.Property(x => x.Status).HasMaxLength(40).IsRequired();
            builder.Property(x => x.LastErrorSummary).HasMaxLength(500);
            builder.HasIndex(x => new { x.TenantId, x.WebhookSubscriptionId, x.TriggeredAtUtc });
        });

        base.OnModelCreating(modelBuilder);
    }

    private ValueConverter<string, string> CreateProtectedStringConverter()
        => new(
            value => _secretProtector.Protect(value ?? string.Empty),
            value => string.IsNullOrWhiteSpace(value) ? string.Empty : _secretProtector.Unprotect(value));

    private ValueConverter<string?, string?> CreateProtectedNullableStringConverter()
        => new(
            value => string.IsNullOrWhiteSpace(value) ? null : _secretProtector.Protect(value),
            value => string.IsNullOrWhiteSpace(value) ? null : _secretProtector.Unprotect(value));
}
