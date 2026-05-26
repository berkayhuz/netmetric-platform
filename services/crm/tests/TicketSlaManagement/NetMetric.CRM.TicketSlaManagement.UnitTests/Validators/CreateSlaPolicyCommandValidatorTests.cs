// <copyright file="CreateSlaPolicyCommandValidatorTests.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentAssertions;
using NetMetric.CRM.TicketSlaManagement.Application.Commands.SlaPolicies;
using NetMetric.CRM.TicketSlaManagement.Application.Validators;

namespace NetMetric.CRM.TicketSlaManagement.UnitTests.Validators;

public sealed class CreateSlaPolicyCommandValidatorTests
{
    [Fact]
    public void Validate_Should_Fail_When_Targets_Are_Invalid()
    {
        var validator = new CreateSlaPolicyCommandValidator();
        var command = new CreateSlaPolicyCommand("Gold", null, 1, 120, 60, false);

        validator.Validate(command).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_Should_Pass_When_Request_Is_Valid()
    {
        var validator = new CreateSlaPolicyCommandValidator();
        var command = new CreateSlaPolicyCommand("Gold", null, 1, 60, 240, true);

        validator.Validate(command).IsValid.Should().BeTrue();
    }
}
