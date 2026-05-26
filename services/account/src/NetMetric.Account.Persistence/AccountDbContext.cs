// <copyright file="AccountDbContext.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using NetMetric.Account.Application.Abstractions.Persistence;
using NetMetric.Account.Domain.Audit;
using NetMetric.Account.Domain.Consents;
using NetMetric.Account.Domain.Devices;
using NetMetric.Account.Domain.Notifications;
using NetMetric.Account.Domain.Outbox;
using NetMetric.Account.Domain.Preferences;
using NetMetric.Account.Domain.Profiles;
using NetMetric.Account.Domain.Sessions;

namespace NetMetric.Account.Persistence;

public sealed class AccountDbContext(DbContextOptions<AccountDbContext> options) : DbContext(options), IAccountDbContext
{
    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();
    public DbSet<AccountMediaAsset> MediaAssets => Set<AccountMediaAsset>();
    public DbSet<UserPreference> UserPreferences => Set<UserPreference>();
    public DbSet<NotificationPreference> NotificationPreferences => Set<NotificationPreference>();
    public DbSet<UserNotificationState> UserNotificationStates => Set<UserNotificationState>();
    public DbSet<UserSession> UserSessions => Set<UserSession>();
    public DbSet<AccountAuditEntry> AccountAuditEntries => Set<AccountAuditEntry>();
    public DbSet<TrustedDevice> TrustedDevices => Set<TrustedDevice>();
    public DbSet<UserConsent> UserConsents => Set<UserConsent>();
    public DbSet<AccountOutboxMessage> OutboxMessages => Set<AccountOutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AccountDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
