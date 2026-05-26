// <copyright file="UpsertLeadScoreCommandValidatorTests.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentAssertions;
using NetMetric.CRM.LeadManagement.Application.Commands.Leads;
using NetMetric.CRM.LeadManagement.Application.Validators;

namespace NetMetric.CRM.LeadManagement.UnitTests.Validators;

public sealed class UpsertLeadScoreCommandValidatorTests
{
    private readonly UpsertLeadScoreCommandValidator _validator = new();

    [Theory]
    [InlineData(-1)]
    [InlineData(101)]
    public void Validate_Should_Fail_When_Score_Is_Out_Of_Range(decimal score)
    {
        var command = new UpsertLeadScoreCommand(Guid.NewGuid(), score, null);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_Should_Pass_When_Score_Is_In_Range()
    {
        var command = new UpsertLeadScoreCommand(Guid.NewGuid(), 87, "Qualified by SDR");

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }
}
