// <copyright file="GetSalesForecastSummaryQueryHandler.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using MediatR;
using Microsoft.EntityFrameworkCore;
using NetMetric.CRM.SalesForecasting.Application.Abstractions.Persistence;
using NetMetric.CRM.SalesForecasting.Application.Common;
using NetMetric.CRM.SalesForecasting.Application.Queries;
using NetMetric.CRM.SalesForecasting.Contracts.DTOs;
using NetMetric.CRM.SalesForecasting.Contracts.Enums;

namespace NetMetric.CRM.SalesForecasting.Application.Handlers;

public sealed class GetSalesForecastSummaryQueryHandler(ISalesForecastingDbContext dbContext) : IRequestHandler<GetSalesForecastSummaryQuery, SalesForecastSummaryDto>
{
    public async Task<SalesForecastSummaryDto> Handle(GetSalesForecastSummaryQuery request, CancellationToken cancellationToken)
    {
        var opportunities = await SalesForecastingQueryHelper.BuildOpportunityQuery(dbContext, request.PeriodStart, request.PeriodEnd, request.OwnerUserId).ToListAsync(cancellationToken);
        var deals = await SalesForecastingQueryHelper.BuildDealQuery(dbContext, request.PeriodStart, request.PeriodEnd, request.OwnerUserId).ToListAsync(cancellationToken);
        var quotas = await SalesForecastingQueryHelper.BuildQuotaQuery(dbContext, request.PeriodStart, request.PeriodEnd, request.OwnerUserId).ToListAsync(cancellationToken);
        var adjustments = await SalesForecastingQueryHelper.BuildAdjustmentQuery(dbContext, request.PeriodStart, request.PeriodEnd, request.OwnerUserId).ToListAsync(cancellationToken);

        var pipelineAmount = opportunities.Sum(x => x.EstimatedAmount);
        var weightedAmount = opportunities.Sum(x => SalesForecastCalculator.WeightedAmount(x.EstimatedAmount, x.Probability));
        var bestCaseAmount = opportunities.Where(x => SalesForecastCalculator.Classify(x) is ForecastBucketType.BestCase or ForecastBucketType.Commit or ForecastBucketType.ClosedWon).Sum(x => x.EstimatedAmount);
        var commitAmount = opportunities.Where(x => SalesForecastCalculator.Classify(x) is ForecastBucketType.Commit or ForecastBucketType.ClosedWon).Sum(x => x.EstimatedAmount);
        var closedWonAmount = deals.Sum(x => x.TotalAmount);
        var quotaAmount = quotas.Sum(x => x.Amount);
        var adjustmentAmount = adjustments.Sum(x => x.Amount);

        var ownerSummaries = opportunities
            .GroupBy(x => x.OwnerUserId)
            .Select(group =>
            {
                var ownerDeals = deals.Where(x => x.OwnerUserId == group.Key).ToList();
                var ownerQuotas = quotas.Where(x => x.OwnerUserId == group.Key).ToList();
                var ownerPipeline = group.Sum(x => x.EstimatedAmount);
                var ownerWeighted = group.Sum(x => SalesForecastCalculator.WeightedAmount(x.EstimatedAmount, x.Probability));
                var ownerBestCase = group.Where(x => SalesForecastCalculator.Classify(x) is ForecastBucketType.BestCase or ForecastBucketType.Commit or ForecastBucketType.ClosedWon).Sum(x => x.EstimatedAmount);
                var ownerCommit = group.Where(x => SalesForecastCalculator.Classify(x) is ForecastBucketType.Commit or ForecastBucketType.ClosedWon).Sum(x => x.EstimatedAmount);
                var ownerClosedWon = ownerDeals.Sum(x => x.TotalAmount);
                var ownerQuota = ownerQuotas.Sum(x => x.Amount);

                return new SalesForecastOwnerSummaryDto(
                    group.Key,
                    ownerPipeline,
                    ownerWeighted,
                    ownerBestCase,
                    ownerCommit,
                    ownerClosedWon,
                    ownerQuota,
                    SalesForecastCalculator.Ratio(ownerPipeline, ownerQuota),
                    SalesForecastCalculator.Ratio(ownerClosedWon, ownerQuota));
            })
            .OrderByDescending(x => x.WeightedPipelineAmount)
            .ToList();

        return new SalesForecastSummaryDto(
            request.PeriodStart,
            request.PeriodEnd,
            pipelineAmount,
            weightedAmount,
            bestCaseAmount,
            commitAmount,
            closedWonAmount,
            quotaAmount,
            adjustmentAmount,
            SalesForecastCalculator.Ratio(weightedAmount + adjustmentAmount, quotaAmount),
            SalesForecastCalculator.Ratio(closedWonAmount, quotaAmount),
            ownerSummaries);
    }
}
