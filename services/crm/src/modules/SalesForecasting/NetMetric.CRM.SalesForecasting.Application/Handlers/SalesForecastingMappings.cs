// <copyright file="SalesForecastingMappings.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.CRM.Sales;
using NetMetric.CRM.SalesForecasting.Application.Common;
using NetMetric.CRM.SalesForecasting.Contracts.DTOs;
using NetMetric.CRM.SalesForecasting.Domain.Entities;

namespace NetMetric.CRM.SalesForecasting.Application.Handlers;

internal static class SalesForecastingMappings
{
    public static SalesQuotaDto ToDto(this SalesQuota entity)
        => new(entity.Id, entity.PeriodStart, entity.PeriodEnd, entity.OwnerUserId, entity.Amount, entity.CurrencyCode, entity.Notes, Convert.ToBase64String(entity.RowVersion));

    public static ForecastAdjustmentDto ToDto(this ForecastAdjustment entity)
        => new(entity.Id, entity.PeriodStart, entity.PeriodEnd, entity.OwnerUserId, entity.Amount, entity.Reason, entity.Notes, Convert.ToBase64String(entity.RowVersion));

    public static ForecastSnapshotDto ToDto(this ForecastSnapshot entity)
        => new(entity.Id, entity.Name, entity.PeriodStart, entity.PeriodEnd, entity.OwnerUserId, entity.ForecastCategory, entity.PipelineAmount, entity.WeightedPipelineAmount, entity.BestCaseAmount, entity.CommitAmount, entity.ClosedWonAmount, entity.QuotaAmount, entity.AdjustmentAmount, entity.Notes, Convert.ToBase64String(entity.RowVersion));

    public static OpportunityForecastRowDto ToForecastRow(this Opportunity opportunity)
    {
        var bucket = SalesForecastCalculator.Classify(opportunity);
        return new(
            opportunity.Id,
            opportunity.OpportunityCode,
            opportunity.Name,
            opportunity.OwnerUserId,
            opportunity.EstimatedCloseDate,
            opportunity.EstimatedAmount,
            opportunity.Probability,
            bucket,
            SalesForecastCalculator.WeightedAmount(opportunity.EstimatedAmount, opportunity.Probability));
    }
}
