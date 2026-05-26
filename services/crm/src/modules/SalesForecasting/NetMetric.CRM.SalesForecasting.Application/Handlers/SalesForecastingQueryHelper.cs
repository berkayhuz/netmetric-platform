// <copyright file="SalesForecastingQueryHelper.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.CRM.Sales;
using NetMetric.CRM.SalesForecasting.Application.Abstractions.Persistence;
using NetMetric.CRM.SalesForecasting.Domain.Entities;
using NetMetric.CRM.Types;

namespace NetMetric.CRM.SalesForecasting.Application.Handlers;

internal static class SalesForecastingQueryHelper
{
    public static IQueryable<Opportunity> BuildOpportunityQuery(ISalesForecastingDbContext dbContext, DateOnly start, DateOnly end, Guid? ownerUserId)
    {
        var startDate = start.ToDateTime(TimeOnly.MinValue);
        var endDateExclusive = end.AddDays(1).ToDateTime(TimeOnly.MinValue);

        var query = dbContext.Opportunities
            .Where(x => x.Status == OpportunityStatusType.Open
                        && x.EstimatedCloseDate.HasValue
                        && x.EstimatedCloseDate.Value >= startDate
                        && x.EstimatedCloseDate.Value < endDateExclusive);

        if (ownerUserId.HasValue)
            query = query.Where(x => x.OwnerUserId == ownerUserId);

        return query;
    }

    public static IQueryable<Deal> BuildDealQuery(ISalesForecastingDbContext dbContext, DateOnly start, DateOnly end, Guid? ownerUserId)
    {
        var startDate = start.ToDateTime(TimeOnly.MinValue);
        var endDateExclusive = end.AddDays(1).ToDateTime(TimeOnly.MinValue);

        var query = dbContext.Deals
            .Where(x => x.ClosedDate >= startDate && x.ClosedDate < endDateExclusive);

        if (ownerUserId.HasValue)
            query = query.Where(x => x.OwnerUserId == ownerUserId);

        return query;
    }

    public static IQueryable<SalesQuota> BuildQuotaQuery(ISalesForecastingDbContext dbContext, DateOnly start, DateOnly end, Guid? ownerUserId)
    {
        var query = dbContext.SalesQuotas.Where(x => x.PeriodStart == start && x.PeriodEnd == end);

        if (ownerUserId.HasValue)
            query = query.Where(x => x.OwnerUserId == ownerUserId);

        return query;
    }

    public static IQueryable<ForecastAdjustment> BuildAdjustmentQuery(ISalesForecastingDbContext dbContext, DateOnly start, DateOnly end, Guid? ownerUserId)
    {
        var query = dbContext.ForecastAdjustments.Where(x => x.PeriodStart == start && x.PeriodEnd == end);

        if (ownerUserId.HasValue)
            query = query.Where(x => x.OwnerUserId == ownerUserId);

        return query;
    }

    public static IQueryable<ForecastSnapshot> BuildSnapshotQuery(ISalesForecastingDbContext dbContext, DateOnly start, DateOnly end, Guid? ownerUserId)
    {
        var query = dbContext.ForecastSnapshots.Where(x => x.PeriodStart == start && x.PeriodEnd == end);

        if (ownerUserId.HasValue)
            query = query.Where(x => x.OwnerUserId == ownerUserId);

        return query.OrderByDescending(x => x.CreatedAt);
    }
}
