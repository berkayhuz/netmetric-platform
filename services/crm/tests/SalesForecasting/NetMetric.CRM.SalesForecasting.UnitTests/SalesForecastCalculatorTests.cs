// <copyright file="SalesForecastCalculatorTests.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentAssertions;
using NetMetric.CRM.Sales;
using NetMetric.CRM.SalesForecasting.Application.Common;
using NetMetric.CRM.SalesForecasting.Contracts.Enums;
using NetMetric.CRM.Types;

namespace NetMetric.CRM.SalesForecasting.UnitTests;

public sealed class SalesForecastCalculatorTests
{
    [Theory]
    [InlineData(OpportunityStageType.Prospecting, 20, ForecastBucketType.Pipeline)]
    [InlineData(OpportunityStageType.Proposal, 40, ForecastBucketType.BestCase)]
    [InlineData(OpportunityStageType.Qualification, 60, ForecastBucketType.BestCase)]
    [InlineData(OpportunityStageType.Negotiation, 70, ForecastBucketType.Commit)]
    [InlineData(OpportunityStageType.Qualification, 80, ForecastBucketType.Commit)]
    public void Classify_Should_Return_Expected_Bucket(OpportunityStageType stage, decimal probability, ForecastBucketType expected)
    {
        var opportunity = new Opportunity
        {
            OpportunityCode = "OPP-1",
            Name = "Test",
            Stage = stage,
            Probability = probability,
            Status = OpportunityStatusType.Open
        };

        SalesForecastCalculator.Classify(opportunity).Should().Be(expected);
    }

    [Fact]
    public void WeightedAmount_Should_Calculate_Using_Probability()
    {
        SalesForecastCalculator.WeightedAmount(1000m, 37.5m).Should().Be(375.00m);
    }

    [Fact]
    public void Ratio_Should_Return_Zero_When_Denominator_Is_Zero()
    {
        SalesForecastCalculator.Ratio(100m, 0m).Should().Be(0m);
    }
}
