// <copyright file="OpportunityManagementDbContext.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.Activities;
using NetMetric.CRM.Core;
using NetMetric.CRM.OpportunityManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.OpportunityManagement.Infrastructure.Outbox;
using NetMetric.CRM.Sales;
using NetMetric.Entities.Abstractions;
using NetMetric.Persistence.EntityFrameworkCore;
using NetMetric.Tenancy;

namespace NetMetric.CRM.OpportunityManagement.Infrastructure.Persistence;

public sealed class OpportunityManagementDbContext : DbContext, IOpportunityManagementDbContext
{
    private readonly ITenantProvider _tenantProvider;

    public OpportunityManagementDbContext(DbContextOptions<OpportunityManagementDbContext> options, ITenantProvider tenantProvider) : base(options)
    {
        _tenantProvider = tenantProvider;
    }

    public Guid? CurrentTenantId => _tenantProvider.TenantId;
    public DbSet<Opportunity> Opportunities => Set<Opportunity>();
    public DbSet<OpportunityProduct> OpportunityProducts => Set<OpportunityProduct>();
    public DbSet<OpportunityContact> OpportunityContacts => Set<OpportunityContact>();
    public DbSet<OpportunityStageHistory> OpportunityStageHistories => Set<OpportunityStageHistory>();
    public DbSet<Quote> Quotes => Set<Quote>();
    public DbSet<QuoteItem> QuoteItems => Set<QuoteItem>();
    public DbSet<LostReason> LostReasons => Set<LostReason>();
    public DbSet<Deal> Deals => Set<Deal>();
    public DbSet<Activity> Activities => Set<Activity>();
    public DbSet<GlobalTrashItem> GlobalTrashItems => Set<GlobalTrashItem>();
    public DbSet<OpportunityManagementOutboxMessage> OutboxMessages => Set<OpportunityManagementOutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OpportunityManagementDbContext).Assembly);
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
                filterBody = Expression.Equal(isDeletedProperty, Expression.Constant(false));
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
                filterBody = filterBody is null ? tenantFilterExpression : Expression.AndAlso(filterBody, tenantFilterExpression);
            }

            if (filterBody is null)
                continue;

            var lambda = Expression.Lambda(filterBody, parameter);
            modelBuilder.Entity(clrType).HasQueryFilter(lambda);
        }
    }
}
