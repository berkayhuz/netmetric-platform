// <copyright file="ISalesForecastingDbContext.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.Sales;
using NetMetric.CRM.SalesForecasting.Domain.Entities;

namespace NetMetric.CRM.SalesForecasting.Application.Abstractions.Persistence;

public interface ISalesForecastingDbContext
{
    DbSet<Opportunity> Opportunities { get; }
    DbSet<Deal> Deals { get; }
    DbSet<SalesQuota> SalesQuotas { get; }
    DbSet<ForecastAdjustment> ForecastAdjustments { get; }
    DbSet<ForecastSnapshot> ForecastSnapshots { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
