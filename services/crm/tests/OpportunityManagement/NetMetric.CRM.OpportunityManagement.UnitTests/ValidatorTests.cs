// <copyright file="ValidatorTests.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentAssertions;
using NetMetric.CRM.OpportunityManagement.Application.Commands;
using NetMetric.CRM.OpportunityManagement.Application.Features.Quotes.Commands.CreateQuote;
using NetMetric.CRM.OpportunityManagement.Application.Validators;
using NetMetric.CRM.Types;

namespace NetMetric.CRM.OpportunityManagement.UnitTests;

public sealed class ValidatorTests
{
    [Fact]
    public void CreateOpportunityValidator_Should_Pass_For_Valid_Command()
    {
        var validator = new CreateOpportunityCommandValidator();
        var command = new CreateOpportunityCommand("OPP-001", "ACME Renewal", "Renewal", 100_000m, 70_000m, 70m, DateTime.UtcNow.AddDays(30), OpportunityStageType.Qualification, OpportunityStatusType.Open, PriorityType.High, null, Guid.NewGuid(), Guid.NewGuid(), "Important");
        validator.Validate(command).IsValid.Should().BeTrue();
    }

    [Fact]
    public void CreateQuoteValidator_Should_Fail_When_Items_Are_Empty()
    {
        var validator = new CreateQuoteCommandValidator();
        var command = new CreateQuoteCommand(Guid.NewGuid(), "Q-0001", DateTime.UtcNow, DateTime.UtcNow.AddDays(7), null, null, "TRY", 1m, []);
        validator.Validate(command).IsValid.Should().BeFalse();
    }
}
