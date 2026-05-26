// <copyright file="DocumentManagementDbContext.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.DocumentManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.DocumentManagement.Domain.Entities.DocumentAttachments;
using NetMetric.CRM.DocumentManagement.Domain.Entities.DocumentPreviewAssets;
using NetMetric.CRM.DocumentManagement.Domain.Entities.DocumentRecords;
using NetMetric.CRM.DocumentManagement.Domain.Entities.DocumentReviews;
using NetMetric.CRM.DocumentManagement.Domain.Entities.DocumentStorageProviders;
using NetMetric.CRM.DocumentManagement.Domain.Entities.DocumentVersions;
using NetMetric.Persistence.EntityFrameworkCore;
using NetMetric.Persistence.EntityFrameworkCore.Auditing;
using NetMetric.Persistence.EntityFrameworkCore.SoftDelete;
using NetMetric.Persistence.EntityFrameworkCore.Tenancy;
using NetMetric.Tenancy;

namespace NetMetric.CRM.DocumentManagement.Infrastructure.Persistence;

public sealed class DocumentManagementDbContext : AppDbContext, IDocumentManagementDbContext
{
    private readonly AuditSaveChangesInterceptor _auditInterceptor;
    private readonly SoftDeleteSaveChangesInterceptor _softDeleteInterceptor;
    private readonly TenantIsolationSaveChangesInterceptor _tenantInterceptor;

    public DocumentManagementDbContext(
        DbContextOptions<DocumentManagementDbContext> options,
        ITenantContext tenantContext,
        AuditSaveChangesInterceptor auditInterceptor,
        SoftDeleteSaveChangesInterceptor softDeleteInterceptor,
        TenantIsolationSaveChangesInterceptor tenantInterceptor) : base(options, tenantContext)
    {
        _auditInterceptor = auditInterceptor;
        _softDeleteInterceptor = softDeleteInterceptor;
        _tenantInterceptor = tenantInterceptor;
    }

    public DbSet<DocumentRecord> DocumentRecords => Set<DocumentRecord>();
    public DbSet<DocumentAttachment> DocumentAttachments => Set<DocumentAttachment>();
    public DbSet<DocumentVersion> DocumentVersions => Set<DocumentVersion>();
    public DbSet<DocumentPreviewAsset> DocumentPreviewAssets => Set<DocumentPreviewAsset>();
    public DbSet<DocumentReview> DocumentReviews => Set<DocumentReview>();
    public DbSet<DocumentStorageProvider> DocumentStorageProviders => Set<DocumentStorageProvider>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.AddInterceptors(_tenantInterceptor, _auditInterceptor, _softDeleteInterceptor);
        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DocumentManagementDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
