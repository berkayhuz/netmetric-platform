// <copyright file="CustomerManagementDbContext.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.Activities;
using NetMetric.CRM.Auditing;
using NetMetric.CRM.Core;
using NetMetric.CRM.CustomerManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.CustomerManagement.Domain.Entities.CustomerOperations;
using NetMetric.CRM.CustomerManagement.Domain.Outbox;
using NetMetric.CRM.CustomFields;
using NetMetric.CRM.Documents;
using NetMetric.CRM.Tagging;
using NetMetric.Entities.Abstractions;
using NetMetric.Persistence.EntityFrameworkCore;
using NetMetric.Tenancy;

namespace NetMetric.CRM.CustomerManagement.Infrastructure.Persistence;

public sealed class CustomerManagementDbContext : DbContext, ICustomerManagementDbContext
{
    private readonly ITenantProvider _tenantProvider;

    public CustomerManagementDbContext(
        DbContextOptions<CustomerManagementDbContext> options,
        ITenantProvider tenantProvider) : base(options)
    {
        _tenantProvider = tenantProvider;
    }

    public Guid? CurrentTenantId => _tenantProvider.TenantId;

    public DbSet<Company> Companies => Set<Company>();
    public DbSet<Contact> Contacts => Set<Contact>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<CustomerStakeholder> CustomerStakeholders => Set<CustomerStakeholder>();
    public DbSet<CustomerConsent> CustomerConsents => Set<CustomerConsent>();
    public DbSet<CustomerConsentHistory> CustomerConsentHistories => Set<CustomerConsentHistory>();
    public DbSet<CustomerLifecycleStageHistory> CustomerLifecycleStageHistories => Set<CustomerLifecycleStageHistory>();
    public DbSet<CustomerEnrichmentProfile> CustomerEnrichmentProfiles => Set<CustomerEnrichmentProfile>();
    public DbSet<CustomerDataQualitySnapshot> CustomerDataQualitySnapshots => Set<CustomerDataQualitySnapshot>();
    public DbSet<CustomerTerritory> CustomerTerritories => Set<CustomerTerritory>();
    public DbSet<CustomerOwnershipRule> CustomerOwnershipRules => Set<CustomerOwnershipRule>();
    public DbSet<CustomerOwnershipAssignmentHistory> CustomerOwnershipAssignmentHistories => Set<CustomerOwnershipAssignmentHistory>();
    public DbSet<CustomerImportBatch> CustomerImportBatches => Set<CustomerImportBatch>();
    public DbSet<CustomerImportRow> CustomerImportRows => Set<CustomerImportRow>();
    public DbSet<CustomerRecordShare> CustomerRecordShares => Set<CustomerRecordShare>();
    public DbSet<CustomerAuditEvent> CustomerAuditEvents => Set<CustomerAuditEvent>();
    public DbSet<CustomerSearchDocument> CustomerSearchDocuments => Set<CustomerSearchDocument>();
    public DbSet<CustomerRelationshipHealthSnapshot> CustomerRelationshipHealthSnapshots => Set<CustomerRelationshipHealthSnapshot>();
    public DbSet<Address> Addresses => Set<Address>();
    public DbSet<Note> Notes => Set<Note>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<TagMap> TagMaps => Set<TagMap>();
    public DbSet<Document> Documents => Set<Document>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<CustomerManagementOutboxMessage> OutboxMessages => Set<CustomerManagementOutboxMessage>();
    public DbSet<CustomFieldDefinition> CustomFieldDefinitions => Set<CustomFieldDefinition>();
    public DbSet<CustomFieldOption> CustomFieldOptions => Set<CustomFieldOption>();
    public DbSet<CustomFieldValue> CustomFieldValues => Set<CustomFieldValue>();
    public DbSet<GlobalTrashItem> GlobalTrashItems => Set<GlobalTrashItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CustomerManagementDbContext).Assembly);
        modelBuilder.ApplyDefaultDecimalPrecision();
        ApplyGlobalFilters(modelBuilder);
    }

    private void ApplyGlobalFilters(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var clrType = entityType.ClrType;
            var parameter = Expression.Parameter(clrType, "e");
            Expression? filterBody = null;

            if (typeof(ISoftDeletable).IsAssignableFrom(clrType))
            {
                var isDeletedProperty = Expression.Property(parameter, nameof(ISoftDeletable.IsDeleted));
                var notDeletedExpression = Expression.Equal(isDeletedProperty, Expression.Constant(false));
                filterBody = notDeletedExpression;
            }

            if (typeof(ITenantEntity).IsAssignableFrom(clrType))
            {
                var tenantProperty = Expression.Property(parameter, nameof(ITenantEntity.TenantId));
                var dbContextExpression = Expression.Constant(this);
                var currentTenantExpression = Expression.Property(dbContextExpression, nameof(CurrentTenantId));
                var hasValueExpression = Expression.Property(currentTenantExpression, nameof(Nullable<Guid>.HasValue));
                var getValueOrDefaultMethod = typeof(Guid?).GetMethod(nameof(Nullable<Guid>.GetValueOrDefault), Type.EmptyTypes)!;
                var tenantValueExpression = Expression.Call(currentTenantExpression, getValueOrDefaultMethod);
                var tenantMatchExpression = Expression.Equal(tenantProperty, tenantValueExpression);
                var tenantFilterExpression = Expression.AndAlso(hasValueExpression, tenantMatchExpression);

                filterBody = filterBody is null
                    ? tenantFilterExpression
                    : Expression.AndAlso(filterBody, tenantFilterExpression);
            }

            if (filterBody is null)
                continue;

            var lambda = Expression.Lambda(filterBody, parameter);
            modelBuilder.Entity(clrType).HasQueryFilter(lambda);
        }
    }
}
