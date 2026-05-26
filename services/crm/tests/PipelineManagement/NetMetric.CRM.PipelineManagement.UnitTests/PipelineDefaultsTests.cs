// <copyright file="PipelineDefaultsTests.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentAssertions;
using NetMetric.CRM.PipelineManagement.Application.Common;
using NetMetric.CRM.Types;

namespace NetMetric.CRM.PipelineManagement.UnitTests;

public sealed class PipelineDefaultsTests
{
    [Theory]
    [InlineData(OpportunityStageType.Prospecting, OpportunityStatusType.Open, 10)]
    [InlineData(OpportunityStageType.Qualification, OpportunityStatusType.Open, 25)]
    [InlineData(OpportunityStageType.Proposal, OpportunityStatusType.Open, 65)]
    [InlineData(OpportunityStageType.Won, OpportunityStatusType.Won, 100)]
    [InlineData(OpportunityStageType.Lost, OpportunityStatusType.Lost, 0)]
    public void ResolveStatus_And_Probability_Should_Map_Stage(OpportunityStageType stage, OpportunityStatusType expectedStatus, decimal expectedProbability)
    {
        PipelineDefaults.ResolveStatus(stage).Should().Be(expectedStatus);
        PipelineDefaults.ResolveProbability(stage).Should().Be(expectedProbability);
    }

    [Fact]
    public void SplitFullName_Should_Split_MultiPart_Name()
    {
        var result = PipelineDefaults.SplitFullName("Berkay Huz");

        result.FirstName.Should().Be("Berkay");
        result.LastName.Should().Be("Huz");
    }
}
