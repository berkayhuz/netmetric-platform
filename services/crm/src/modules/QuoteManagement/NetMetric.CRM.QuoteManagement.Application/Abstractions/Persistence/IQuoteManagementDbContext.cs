// <copyright file="IQuoteManagementDbContext.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.Catalog;
using NetMetric.CRM.Core;
using NetMetric.CRM.QuoteManagement.Domain.Entities;
using NetMetric.CRM.Sales;
using NetMetric.Repository;

namespace NetMetric.CRM.QuoteManagement.Application.Abstractions.Persistence;

public interface IQuoteManagementDbContext : IUnitOfWork
{
    DbSet<Quote> Quotes { get; }
    DbSet<QuoteItem> QuoteItems { get; }
    DbSet<ProposalTemplate> ProposalTemplates { get; }
    DbSet<QuoteStatusHistory> QuoteStatusHistories { get; }
    DbSet<ProductRule> ProductRules { get; }
    DbSet<ProductBundle> ProductBundles { get; }
    DbSet<ProductBundleItem> ProductBundleItems { get; }
    DbSet<GuidedSellingPlaybook> GuidedSellingPlaybooks { get; }
    DbSet<Opportunity> Opportunities { get; }
    DbSet<Customer> Customers { get; }
    DbSet<Product> Products { get; }
    DbSet<GlobalTrashItem> GlobalTrashItems { get; }
}
