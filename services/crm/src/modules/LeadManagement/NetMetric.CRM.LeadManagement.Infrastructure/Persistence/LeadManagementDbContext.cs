// <copyright file="LeadManagementDbContext.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.Core;
using NetMetric.CRM.LeadManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.LeadManagement.Infrastructure.Outbox;
using NetMetric.CRM.Sales;
using NetMetric.Entities.Abstractions;
using NetMetric.Persistence.EntityFrameworkCore;
using NetMetric.Tenancy;

namespace NetMetric.CRM.LeadManagement.Infrastructure.Persistence;

public sealed class LeadManagementDbContext : DbContext, ILeadManagementDbContext
{
    private readonly ITenantProvider _tenantProvider;

    public LeadManagementDbContext(
        DbContextOptions<LeadManagementDbContext> options,
        ITenantProvider tenantProvider) : base(options)
    {
        _tenantProvider = tenantProvider;
    }

    public Guid? CurrentTenantId => _tenantProvider.TenantId;

    public DbSet<Lead> Leads => Set<Lead>();
    public DbSet<LeadScore> LeadScores => Set<LeadScore>();
    public DbSet<LeadOwnershipHistory> LeadOwnershipHistories => Set<LeadOwnershipHistory>();
    public DbSet<Opportunity> Opportunities => Set<Opportunity>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<GlobalTrashItem> GlobalTrashItems => Set<GlobalTrashItem>();
    public DbSet<LeadManagementOutboxMessage> OutboxMessages => Set<LeadManagementOutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(LeadManagementDbContext).Assembly);
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

            modelBuilder.Entity(clrType).HasQueryFilter(Expression.Lambda(filterBody, parameter));
        }
    }
}
