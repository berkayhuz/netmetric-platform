// <copyright file="ICustomerManagementDbContext.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.Core;
using NetMetric.CRM.CustomerManagement.Domain.Entities.CustomerOperations;
using NetMetric.CRM.CustomerManagement.Domain.Outbox;
using NetMetric.Repository;

namespace NetMetric.CRM.CustomerManagement.Application.Abstractions.Persistence;

public interface ICustomerManagementDbContext : IUnitOfWork
{
    DbSet<TEntity> Set<TEntity>() where TEntity : class;
    DbSet<Company> Companies { get; }
    DbSet<Contact> Contacts { get; }
    DbSet<Customer> Customers { get; }
    DbSet<CustomerStakeholder> CustomerStakeholders { get; }
    DbSet<CustomerConsent> CustomerConsents { get; }
    DbSet<CustomerConsentHistory> CustomerConsentHistories { get; }
    DbSet<CustomerLifecycleStageHistory> CustomerLifecycleStageHistories { get; }
    DbSet<CustomerEnrichmentProfile> CustomerEnrichmentProfiles { get; }
    DbSet<CustomerDataQualitySnapshot> CustomerDataQualitySnapshots { get; }
    DbSet<CustomerTerritory> CustomerTerritories { get; }
    DbSet<CustomerOwnershipRule> CustomerOwnershipRules { get; }
    DbSet<CustomerOwnershipAssignmentHistory> CustomerOwnershipAssignmentHistories { get; }
    DbSet<CustomerImportBatch> CustomerImportBatches { get; }
    DbSet<CustomerImportRow> CustomerImportRows { get; }
    DbSet<CustomerRecordShare> CustomerRecordShares { get; }
    DbSet<CustomerAuditEvent> CustomerAuditEvents { get; }
    DbSet<CustomerSearchDocument> CustomerSearchDocuments { get; }
    DbSet<CustomerRelationshipHealthSnapshot> CustomerRelationshipHealthSnapshots { get; }
    DbSet<CustomerManagementOutboxMessage> OutboxMessages { get; }
}
