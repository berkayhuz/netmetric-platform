// <copyright file="DealValidatorTests.cs" company="NetMetric">
// Copyright (c) 2026 NetMetric. All rights reserved.
// NetMetric is proprietary software. See the LICENSE file in the repository root.
// </copyright>

using FluentAssertions;
using NetMetric.CRM.DealManagement.Application.Commands.Deals;
using NetMetric.CRM.DealManagement.Application.Commands.Reviews;
using NetMetric.CRM.DealManagement.Application.Validators;

namespace NetMetric.CRM.DealManagement.UnitTests;

public sealed class DealValidatorTests
{
    [Fact]
    public void CreateDealValidator_Should_Pass_For_Valid_Request()
    {
        var validator = new CreateDealCommandValidator();
        var command = new CreateDealCommand("D-2026-0001", "Big Deal", 150000m, DateTime.UtcNow, Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), "Strategic");
        validator.Validate(command).IsValid.Should().BeTrue();
    }

    [Fact]
    public void UpdateDealValidator_Should_Pass_When_RowVersion_Is_Empty()
    {
        var validator = new UpdateDealCommandValidator();
        var command = new UpdateDealCommand(Guid.NewGuid(), "D-1", "Deal", 1m, DateTime.UtcNow, null, null, null, null, string.Empty);
        validator.Validate(command).IsValid.Should().BeTrue();
    }

    [Fact]
    public void UpsertWinLossReviewValidator_Should_Pass_For_Valid_Request()
    {
        var validator = new UpsertWinLossReviewCommandValidator();
        var command = new UpsertWinLossReviewCommand(Guid.NewGuid(), "Lost", "Summary", "Strength", "Risk", "Competitor", 1200m, "Feedback", Convert.ToBase64String([1, 2, 3]));
        validator.Validate(command).IsValid.Should().BeTrue();
    }
}
