// <copyright file="CreateLeadCommandValidatorTests.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentAssertions;
using NetMetric.CRM.LeadManagement.Application.Commands.Leads;
using NetMetric.CRM.LeadManagement.Application.Validators;
using NetMetric.CRM.Types;

namespace NetMetric.CRM.LeadManagement.UnitTests.Validators;

public sealed class CreateLeadCommandValidatorTests
{
    private readonly CreateLeadCommandValidator _validator = new();

    [Fact]
    public void Validate_Should_Pass_For_Valid_Request()
    {
        var command = new CreateLeadCommand(
            "Ada Lovelace",
            "Analytical Engines Ltd",
            "ada@example.com",
            "+905551112233",
            "CTO",
            "Important prospect",
            5000m,
            DateTime.UtcNow.AddDays(2),
            LeadSourceType.Website,
            LeadStatusType.New,
            PriorityType.High,
            null,
            Guid.NewGuid(),
            "Test");

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_Should_Fail_For_Empty_FullName()
    {
        var command = new CreateLeadCommand(
            "",
            null,
            "ada@example.com",
            null,
            null,
            null,
            null,
            null,
            LeadSourceType.Manual,
            LeadStatusType.New,
            PriorityType.Medium,
            null,
            null,
            null);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
    }
}
