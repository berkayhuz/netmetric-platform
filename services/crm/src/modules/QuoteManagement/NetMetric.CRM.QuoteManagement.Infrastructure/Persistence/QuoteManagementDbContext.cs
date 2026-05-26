// <copyright file="QuoteManagementDbContext.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.Catalog;
using NetMetric.CRM.Core;
using NetMetric.CRM.QuoteManagement.Application.Abstractions.Persistence;
using NetMetric.CRM.QuoteManagement.Domain.Entities;
using NetMetric.CRM.QuoteManagement.Infrastructure.Outbox;
using NetMetric.CRM.Sales;
using NetMetric.Entities.Abstractions;
using NetMetric.Persistence.EntityFrameworkCore;
using NetMetric.Tenancy;

namespace NetMetric.CRM.QuoteManagement.Infrastructure.Persistence;

public sealed class QuoteManagementDbContext : DbContext, IQuoteManagementDbContext
{
    private readonly ITenantProvider _tenantProvider;

    public QuoteManagementDbContext(DbContextOptions<QuoteManagementDbContext> options, ITenantProvider tenantProvider) : base(options)
    {
        _tenantProvider = tenantProvider;
    }

    public Guid? CurrentTenantId => _tenantProvider.TenantId;
    public DbSet<Quote> Quotes => Set<Quote>();
    public DbSet<QuoteItem> QuoteItems => Set<QuoteItem>();
    public DbSet<ProposalTemplate> ProposalTemplates => Set<ProposalTemplate>();
    public DbSet<QuoteStatusHistory> QuoteStatusHistories => Set<QuoteStatusHistory>();
    public DbSet<ProductRule> ProductRules => Set<ProductRule>();
    public DbSet<ProductBundle> ProductBundles => Set<ProductBundle>();
    public DbSet<ProductBundleItem> ProductBundleItems => Set<ProductBundleItem>();
    public DbSet<GuidedSellingPlaybook> GuidedSellingPlaybooks => Set<GuidedSellingPlaybook>();
    public DbSet<QuoteManagementOutboxMessage> OutboxMessages => Set<QuoteManagementOutboxMessage>();
    public DbSet<Opportunity> Opportunities => Set<Opportunity>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<GlobalTrashItem> GlobalTrashItems => Set<GlobalTrashItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(QuoteManagementDbContext).Assembly);
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
            entityType.SetQueryFilter(lambda);
        }
    }
}
