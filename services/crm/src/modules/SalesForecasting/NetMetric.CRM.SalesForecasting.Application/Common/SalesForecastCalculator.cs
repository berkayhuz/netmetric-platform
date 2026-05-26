// <copyright file="SalesForecastCalculator.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using NetMetric.CRM.Sales;
using NetMetric.CRM.SalesForecasting.Contracts.Enums;
using NetMetric.CRM.Types;

namespace NetMetric.CRM.SalesForecasting.Application.Common;

public static class SalesForecastCalculator
{
    public static ForecastBucketType Classify(Opportunity opportunity)
    {
        if (opportunity.Status == OpportunityStatusType.Won || opportunity.Stage == OpportunityStageType.Won)
            return ForecastBucketType.ClosedWon;

        if (opportunity.Stage == OpportunityStageType.Negotiation || opportunity.Probability >= 75m)
            return ForecastBucketType.Commit;

        if (opportunity.Stage == OpportunityStageType.Proposal || opportunity.Probability >= 50m)
            return ForecastBucketType.BestCase;

        return ForecastBucketType.Pipeline;
    }

    public static decimal WeightedAmount(decimal estimatedAmount, decimal probability)
        => Math.Round(estimatedAmount * (probability / 100m), 2, MidpointRounding.AwayFromZero);

    public static decimal Ratio(decimal numerator, decimal denominator)
        => denominator <= 0m ? 0m : Math.Round(numerator / denominator, 4, MidpointRounding.AwayFromZero);
}
