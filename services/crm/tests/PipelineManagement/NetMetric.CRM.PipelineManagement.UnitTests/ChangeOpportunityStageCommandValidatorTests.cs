// <copyright file="ChangeOpportunityStageCommandValidatorTests.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentAssertions;
using NetMetric.CRM.PipelineManagement.Application.Commands;
using NetMetric.CRM.PipelineManagement.Application.Validators;
using NetMetric.CRM.Types;

namespace NetMetric.CRM.PipelineManagement.UnitTests;

public sealed class ChangeOpportunityStageCommandValidatorTests
{
    [Fact]
    public void Validate_Should_Fail_When_LostReason_Is_Missing_For_Lost_Stage()
    {
        var validator = new ChangeOpportunityStageCommandValidator();
        var command = new ChangeOpportunityStageCommand(Guid.NewGuid(), OpportunityStageType.Lost, null, null, null, null, null);

        var result = validator.Validate(command);

        result.IsValid.Should().BeFalse();
    }
}
