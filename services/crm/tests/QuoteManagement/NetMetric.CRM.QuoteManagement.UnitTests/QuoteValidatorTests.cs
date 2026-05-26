// <copyright file="QuoteValidatorTests.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentAssertions;
using NetMetric.CRM.QuoteManagement.Application.Commands.Quotes;
using NetMetric.CRM.QuoteManagement.Application.Common;
using NetMetric.CRM.QuoteManagement.Application.Validators;

namespace NetMetric.CRM.QuoteManagement.UnitTests;

public sealed class QuoteValidatorTests
{
    [Fact]
    public void CreateQuoteValidator_Should_Pass_For_Valid_Request()
    {
        var validator = new CreateQuoteCommandValidator();
        var command = new CreateQuoteCommand(
            "Q-2026-0001",
            "Proposal",
            "Summary",
            "Body",
            DateTime.UtcNow,
            DateTime.UtcNow.AddDays(7),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "TRY",
            1,
            "Terms",
            null,
            [new QuoteLineInput(Guid.NewGuid(), "Line", 2, 100, 10, 20)]);

        validator.Validate(command).IsValid.Should().BeTrue();
    }

    [Fact]
    public void CreateQuoteValidator_Should_Fail_When_Items_Are_Empty()
    {
        var validator = new CreateQuoteCommandValidator();
        var command = new CreateQuoteCommand("Q-1", null, null, null, DateTime.UtcNow, null, null, null, null, "TRY", 1, null, null, []);
        validator.Validate(command).IsValid.Should().BeFalse();
    }
}
