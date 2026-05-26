// <copyright file="CreateTicketCommandValidatorTests.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentAssertions;
using NetMetric.CRM.TicketManagement.Application.Commands.Tickets;
using NetMetric.CRM.TicketManagement.Application.Validators;
using NetMetric.CRM.Types;

namespace NetMetric.CRM.TicketManagement.UnitTests.Validators;

public sealed class CreateTicketCommandValidatorTests
{
    private readonly CreateTicketCommandValidator _validator = new();

    [Fact]
    public void Validate_Should_Pass_For_Valid_Request()
    {
        var command = new CreateTicketCommand(
            "Printer is down",
            "Customer cannot print labels.",
            TicketType.Support,
            TicketChannelType.Email,
            PriorityType.High,
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateTime.UtcNow.AddHours(1),
            DateTime.UtcNow.AddHours(6),
            "urgent");

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_Should_Fail_When_Subject_Is_Empty()
    {
        var command = new CreateTicketCommand(
            "",
            null,
            TicketType.Support,
            TicketChannelType.Web,
            PriorityType.Low,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            null);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
    }
}
