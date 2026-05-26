// <copyright file="OmnichannelDbContext.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.Omnichannel.Application.Abstractions.Persistence;
using NetMetric.CRM.Omnichannel.Domain.Entities;
using NetMetric.Persistence.EntityFrameworkCore.Auditing;
using NetMetric.Persistence.EntityFrameworkCore.SoftDelete;
using NetMetric.Persistence.EntityFrameworkCore.Tenancy;
using NetMetric.Tenancy;

namespace NetMetric.CRM.Omnichannel.Infrastructure.Persistence;

public sealed class OmnichannelDbContext : DbContext, IOmnichannelDbContext
{
    private readonly ITenantProvider _tenantProvider;
    private readonly TenantIsolationSaveChangesInterceptor _tenantInterceptor;
    private readonly AuditSaveChangesInterceptor _auditInterceptor;
    private readonly SoftDeleteSaveChangesInterceptor _softDeleteInterceptor;

    public OmnichannelDbContext(
        DbContextOptions<OmnichannelDbContext> options,
        ITenantProvider tenantProvider,
        TenantIsolationSaveChangesInterceptor tenantInterceptor,
        AuditSaveChangesInterceptor auditInterceptor,
        SoftDeleteSaveChangesInterceptor softDeleteInterceptor) : base(options)
    {
        _tenantProvider = tenantProvider;
        _tenantInterceptor = tenantInterceptor;
        _auditInterceptor = auditInterceptor;
        _softDeleteInterceptor = softDeleteInterceptor;
    }

    public DbSet<ChannelAccount> Accounts => Set<ChannelAccount>();
    public DbSet<ChannelConversation> Conversations => Set<ChannelConversation>();
    public DbSet<ChannelMessage> Messages => Set<ChannelMessage>();
    public DbSet<ConversationNote> Notes => Set<ConversationNote>();
    public Guid? CurrentTenantId => _tenantProvider.TenantId;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.AddInterceptors(_tenantInterceptor, _auditInterceptor, _softDeleteInterceptor);
        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OmnichannelDbContext).Assembly);

        modelBuilder.Entity<ChannelAccount>().HasQueryFilter(x => CurrentTenantId.HasValue && !x.IsDeleted && x.TenantId == CurrentTenantId.Value);
        modelBuilder.Entity<ChannelConversation>().HasQueryFilter(x => CurrentTenantId.HasValue && !x.IsDeleted && x.TenantId == CurrentTenantId.Value);
        modelBuilder.Entity<ChannelMessage>().HasQueryFilter(x => CurrentTenantId.HasValue && !x.IsDeleted && x.TenantId == CurrentTenantId.Value);
        modelBuilder.Entity<ConversationNote>().HasQueryFilter(x => CurrentTenantId.HasValue && !x.IsDeleted && x.TenantId == CurrentTenantId.Value);

        base.OnModelCreating(modelBuilder);
    }
}
